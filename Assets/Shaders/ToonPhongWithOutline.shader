Shader "ToonPhongWithOutline"
{
    Properties
    {
        // Base surface
        _BaseMap   ("Base (RGB)", 2D) = "white" {}
        _BaseColor ("Base Colour", Color) = (1,1,1,1)

        // Normal map (optional)
        _BumpMap   ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0,1)) = 0.2

        // Toon lighting
        _Ka    ("Ambient (0..1)", Range(0,1)) = 0.10
        _Kd    ("Diffuse (0..2)", Range(0,2)) = 1.00
        _Ks    ("Specular (0..2)", Range(0,2)) = 0.25
        _Gloss ("Glossiness (0..1)", Range(0,1)) = 0.40
        _DiffuseSteps ("Toon Diffuse Steps", Range(1,8)) = 4
        _SpecSteps    ("Toon Spec Steps",    Range(1,8)) = 2

        // Optional cutout
        [Toggle] _AlphaClipping ("Alpha Clipping", Float) = 0
        _Cutoff ("Cutoff", Range(0,1)) = 0.3

        // Outline (screen-space offset)
        _OutlineWidth ("Outline Width (px)", Range(0.5,8)) = 6
        _ZOffset      ("Depth Offset", Range(-2,2)) = 0.10
        _OutlineColor ("Outline Colour", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags { "Queue"="Geometry+10" "RenderType"="Opaque" }
        LOD 300

        // PASS 1: Outline (screen-space offset)
        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite Off
            ZTest LEqual
            Offset 0, [_ZOffset]
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BaseMap;
            float4 _BaseColor;
            float4 _OutlineColor;
            float  _OutlineWidth;
            half   _AlphaClipping;
            half   _Cutoff;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                // Object to clip
                float4 clipPos = UnityObjectToClipPos(v.vertex);

                // World normal to view space for stable screen offset
                float3 nWorld = UnityObjectToWorldNormal(v.normal);
                float3 nView  = mul((float3x3)UNITY_MATRIX_V, nWorld);
                float2 dir    = normalize(nView.xy + 1e-4);

                // Constant pixel offset
                float2 ndcPerPixel = 1.0 / _ScreenParams.xy;
                float2 offsetNDC   = dir * (_OutlineWidth * ndcPerPixel) * clipPos.w;
                clipPos.xy += offsetNDC;

                v2f o;
                o.pos = clipPos;
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Optional alpha clipping
                fixed4 baseCol = tex2D(_BaseMap, i.uv) * _BaseColor;
                if (_AlphaClipping > 0.5 && baseCol.a < _Cutoff) discard;
                return _OutlineColor;
            }
            ENDCG
        }

        // PASS 2: Main toon surface
        CGPROGRAM
        #pragma surface surf ToonPhong fullforwardshadows addshadow
        #pragma target 3.0

        sampler2D _BaseMap;
        fixed4 _BaseColor;

        sampler2D _BumpMap;
        half _BumpScale;

        half _Ka, _Kd, _Ks, _Gloss;
        half _DiffuseSteps, _SpecSteps;

        half _AlphaClipping;
        half _Cutoff;

        struct Input
        {
            float2 uv_BaseMap;
            float2 uv_BumpMap;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_BaseMap, IN.uv_BaseMap) * _BaseColor;
            if (_AlphaClipping > 0.5 && c.a < _Cutoff) clip(-1);

            fixed3 n = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            n = normalize(lerp(float3(0,0,1), n, saturate(_BumpScale)));
            o.Normal = n;

            o.Albedo = c.rgb;
            o.Alpha  = 1;
        }

        inline half4 LightingToonPhong (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half ndl = saturate(dot(s.Normal, lightDir));
            half dSteps = max(1.0h, _DiffuseSteps);
            half diffQ = floor(ndl * dSteps) / (dSteps - 1.0h);

            half3 h = normalize(lightDir + viewDir);
            half ndh = saturate(dot(s.Normal, h));
            half shininess = lerp(8.0h, 128.0h, saturate(_Gloss));
            half specRaw   = pow(ndh, shininess);

            half sSteps = max(1.0h, _SpecSteps);
            half specQ  = floor(specRaw * sSteps) / (sSteps - 1.0h);

            half3 ambient = _Ka * s.Albedo;
            half3 lit     = _Kd * diffQ * s.Albedo * _LightColor0.rgb * atten
                          + _Ks * specQ            * _LightColor0.rgb * atten;

            return half4(ambient + lit, 1);
        }
        ENDCG
    }

    Fallback "Diffuse"
}
