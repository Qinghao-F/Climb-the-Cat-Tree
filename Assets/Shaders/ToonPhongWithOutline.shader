Shader "ToonPhongWithOutline"
{
    Properties
    {
        // Base painted look
        _BaseMap        ("Base Texture (Colour & Detail)", 2D) = "white" {}
        _BaseColor      ("Overall Tint Colour", Color) = (1,1,1,1)

        // Small-scale bumps
        _BumpMap        ("Normal Map", 2D) = "bump" {}
        _BumpScale      ("Normal Strength", Range(0,1)) = 0.4

        // Keep lit areas colourful instead of bleaching to white
        _SaturationBoost("Colour Saturation Boost", Range(0,1)) = 0.6

        // Outline styling
        _OutlineColour  ("Outline Colour", Color) = (0,0,0,1)
        _OutlineWidth   ("Outline Width (px)", Range(0.5,16)) = 6.0
        _DepthOffset    ("Outline Depth Offset", Range(0,0.01)) = 0.002
        _ZOffset        ("Depth Bias", Range(-2,2)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+10" }

        // -------------------------------------------------
        // PASS 1 — Outline (extruded backfaces as ink)
        // -------------------------------------------------
        Pass
        {
            Cull Front
            ZWrite Off
            ZTest LEqual
            Offset 0, [_ZOffset]

            CGPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "UnityCG.cginc"

            float4 _OutlineColour;
            float  _OutlineWidth;
            float  _DepthOffset;

            struct appdata_outline
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f_outline
            {
                float4 pos : SV_POSITION;
            };

            v2f_outline vertOutline(appdata_outline v)
            {
                // Expand mesh along view-facing normal to draw a cartoon outline around the object.
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldN   = UnityObjectToWorldNormal(v.normal);

                float4 clipPos  = UnityWorldToClipPos(float4(worldPos, 1));
                float3 viewN    = mul((float3x3)UNITY_MATRIX_V, worldN);
                viewN           = normalize(viewN);

                float2 pixelNDC  = 1.0 / _ScreenParams.xy;
                float2 offsetNDC = viewN.xy * (_OutlineWidth * pixelNDC) * clipPos.w;

                clipPos.xy += offsetNDC;
                clipPos.z  -= _DepthOffset * clipPos.w;

                v2f_outline o;
                o.pos = clipPos;
                return o;
            }

            float4 fragOutline(v2f_outline i) : SV_Target
            {
                return _OutlineColour;
            }
            ENDCG
        }

        // -------------------------------------------------
        // PASS 2 — Main toon lighting with GI and shadows
        // -------------------------------------------------
        Pass
        {
            Cull Back
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase      // allow main light / light probes / lightmaps
            #pragma multi_compile_fwdadd       // (keeps forward lighting compatibility)
            #pragma multi_compile_shadowcaster // make sure shadow keywords exist
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            // Artist-facing inputs
            sampler2D _BaseMap;
            float4    _BaseMap_ST;

            sampler2D _BumpMap;
            float4    _BumpMap_ST;
            float     _BumpScale;

            float4    _BaseColor;
            float     _SaturationBoost;

            // Style constants (tuned toon look, not exposed)
            static const float TOON_DIFFUSE_STEPS = 4.0;
            static const float TOON_SPEC_STEPS    = 2.0;
            static const float SPEC_SHARPNESS     = 0.2;   // 0..1 glossiness style
            static const float SPEC_WEIGHT        = 0.3;   // specular brightness
            static const float AMBIENT_WEIGHT     = 1.0;   // GI contribution scale

            struct appdata
            {
                float4 vertex   : POSITION;
                float3 normal   : NORMAL;
                float4 tangent  : TANGENT;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos        : SV_POSITION;
                float2 uvBase     : TEXCOORD0;
                float2 uvBump     : TEXCOORD1;
                float3 worldPos   : TEXCOORD2;
                float3 worldN     : TEXCOORD3;
                float3 worldT     : TEXCOORD4;
                float3 worldB     : TEXCOORD5;
                float3 viewDir    : TEXCOORD6;
                SHADOW_COORDS(7)               // shadow data from main light
            };

            v2f vert(appdata v)
            {
                // Prepare world-space basis for lighting + shadow coords.
                v2f o;
                float4 wp = mul(unity_ObjectToWorld, v.vertex);

                o.pos      = UnityWorldToClipPos(wp);
                o.worldPos = wp.xyz;

                o.worldN   = UnityObjectToWorldNormal(v.normal);
                o.worldT   = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldB   = cross(o.worldN, o.worldT) * v.tangent.w;

                o.viewDir  = normalize(_WorldSpaceCameraPos - o.worldPos);

                o.uvBase   = TRANSFORM_TEX(v.uv, _BaseMap);
                o.uvBump   = TRANSFORM_TEX(v.uv, _BumpMap);

                TRANSFER_SHADOW(o); // pass shadow data to fragment
                return o;
            }

            float3 UnpackNormalScaled(sampler2D map, float2 uv, float scale)
            {
                // Rebuild normal from normal map with adjustable strength for surface detail.
                float3 nTex = UnpackNormal(tex2D(map, uv));
                nTex.xy *= scale;
                nTex.z  = sqrt(saturate(1.0 - dot(nTex.xy, nTex.xy)));
                return nTex;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Base surface colour (texture tinted by chosen material colour).
                float3 baseRGB = tex2D(_BaseMap, i.uvBase).rgb * _BaseColor.rgb;

                // Normal for lighting (combines mesh normal and normal map detail).
                float3 nTan = UnpackNormalScaled(_BumpMap, i.uvBump, _BumpScale);
                float3 wN = normalize(
                    nTan.x * normalize(i.worldT) +
                    nTan.y * normalize(i.worldB) +
                    nTan.z * normalize(i.worldN)
                );

                float3 V = normalize(i.viewDir);
                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float3 H = normalize(L + V);

                // Unity's shadow attenuation from main light.
                float shadowAtten = SHADOW_ATTENUATION(i);

                // Toon diffuse: quantised N·L with shadow applied.
                float ndl   = saturate(dot(wN, L));
                float litNL = ndl * shadowAtten;
                float diffQ = floor(litNL * TOON_DIFFUSE_STEPS) / max(TOON_DIFFUSE_STEPS - 1.0, 1.0);

                // Toon specular: tight banded highlight.
                float ndh = saturate(dot(wN, H));
                float shininess = lerp(8.0, 128.0, SPEC_SHARPNESS);
                float specRaw = pow(ndh, shininess) * shadowAtten;
                float specQ  = floor(specRaw * TOON_SPEC_STEPS) / max(TOON_SPEC_STEPS - 1.0, 1.0);

                // GI / ambient from Unity probes (ShadeSH9).
                float3 giLight = ShadeSH9(float4(wN, 1.0)) * AMBIENT_WEIGHT;

                // Combine banded diffuse + banded spec + GI.
                float3 diffuseTerm  = baseRGB * (_LightColor0.rgb * diffQ + giLight);
                float3 specularTerm = SPEC_WEIGHT * specQ * _LightColor0.rgb;
                float3 litRGB       = diffuseTerm + specularTerm;

                // Keep highlights saturated.
                float3 saturatedRGB = lerp(
                    litRGB,
                    baseRGB,
                    _SaturationBoost * diffQ
                );

                return float4(saturatedRGB, 1.0);
            }
            ENDCG
        }

        // -------------------------------------------------
        // PASS 3 — Shadow caster
        // Lets the mesh cast realtime shadows into the scene.
        // -------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            Cull Back

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                float3 wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 wNorm = UnityObjectToWorldNormal(v.normal);
                o.pos = UnityWorldToClipPos(float4(wPos,1));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }
    }

    Fallback Off
}
