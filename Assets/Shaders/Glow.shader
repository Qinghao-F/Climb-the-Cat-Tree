Shader "Glow"
{
    Properties
    {
        _BaseMap       ("Base (RGB)", 2D) = "white" {}
        _BaseColor     ("Base Color", Color) = (1,1,1,1)
        _EmissionMap   ("Emission (RGB)", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _ALPHABLEND    ("Enable Alpha Blending (0/1)", Range(0,1)) = 1

        [Enum(Off,0,Front,1,Back,2)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        // Transparent unlit visuals
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull [_Cull]
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        // ---- Main unlit (surface) pass: visuals only, no lighting dependence ----
        CGPROGRAM
        // Keep your unlit surface, preserve alpha blending behavior
        #pragma surface surf Unlit keepalpha alpha
        #pragma target 3.0

        sampler2D _BaseMap;
        fixed4 _BaseColor;

        sampler2D _EmissionMap;
        fixed4 _EmissionColor;

        half _ALPHABLEND;

        struct Input
        {
            float2 uv_BaseMap;
            float2 uv_EmissionMap;
        };

        // Combine base color + emission into Emission (unlit)
        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 baseCol = tex2D(_BaseMap, IN.uv_BaseMap) * _BaseColor;
            fixed3 emisTex = tex2D(_EmissionMap, IN.uv_EmissionMap).rgb;
            fixed3 emis    = emisTex * _EmissionColor.rgb;

            o.Emission = baseCol.rgb + emis;

            // If blending is enabled, use base alpha; otherwise render fully opaque
            o.Alpha = (_ALPHABLEND > 0.5h) ? (baseCol.a * _BaseColor.a) : 1.0h;
        }

        // Unlit lighting function: just output Emission/Alpha
        inline fixed4 LightingUnlit (SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            return fixed4(s.Emission, s.Alpha);
        }
        ENDCG

        // ---- ShadowCaster: writes to shadow map so this transparent/unlit object STILL casts shadows ----
        Pass
        {
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            Cull [_Cull]

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { V2F_SHADOW_CASTER; };

            v2f vert (appdata_full v)
            {
                v2f o;
                // Output geometry into the light's shadow map (with normal offset to reduce acne)
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Emit depth for the shadow map (no visible color)
                SHADOW_CASTER_FRAGMENT(i);
            }
            ENDCG
        }
    }

    // Unlit fallback is fine for the visible pass
    Fallback "Unlit/Texture"
}
