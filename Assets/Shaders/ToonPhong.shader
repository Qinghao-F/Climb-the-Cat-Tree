Shader "ToonPhong"
{
    Properties
    {
        _BaseMap   ("Base (RGB)", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _Ka    ("Ambient (0..1)", Range(0,1)) = 0.10
        _Kd    ("Diffuse (0..2)", Range(0,2)) = 1.00
        _Ks    ("Specular (0..2)", Range(0,2)) = 0.25
        _Gloss ("Glossiness (0..1)", Range(0,1)) = 0.40

        _DiffuseSteps ("Toon Diffuse Steps", Range(1,8)) = 4
        _SpecSteps    ("Toon Spec Steps",    Range(1,8)) = 2

        [Enum(Off,0,Front,1,Back,2)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        // Opaque queue for reliable shadowing on ground receivers
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Cull [_Cull]

        CGPROGRAM
        // SURFACE SHADER WITH SHADOWS:
        // - addshadow: generate a ShadowCaster pass
        // - fullforwardshadows: receive forward shadows
        #pragma surface surf ToonFull addshadow fullforwardshadows
        #pragma target 3.0

        sampler2D _BaseMap;
        fixed4 _BaseColor;

        half _Ka, _Kd, _Ks, _Gloss;
        half _DiffuseSteps, _SpecSteps;

        struct Input
        {
            float2 uv_BaseMap;
        };

        // Sample albedo texture + tint
        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_BaseMap, IN.uv_BaseMap) * _BaseColor;
            o.Albedo = c.rgb;
            o.Alpha  = c.a;
        }

        // Quantise a 0..1 value into N bands for toon look
        inline half ToonStep(half x, half steps)
        {
            steps = max(1.0h, steps);
            return floor(saturate(x) * steps) / max(1.0h, (steps - 1.0h));
        }

        // Custom toon Phong lighting.
        // multiply by ‘atten’ to receive shadows.
        half4 LightingToonFull (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half3 N = normalize(s.Normal);
            half3 L = normalize(lightDir);
            half3 V = normalize(viewDir);
            half3 H = normalize(L + V);

            half NdotL = max(0, dot(N, L));
            half diff  = ToonStep(NdotL, _DiffuseSteps);

            half NdotH   = max(0, dot(N, H));
            half shinExp = lerp(8.0h, 128.0h, saturate(_Gloss));
            half shin    = pow(NdotH, shinExp);
            half spec    = ToonStep(shin, _SpecSteps);

            // Ambient (not shadowed), then lit terms * shadow attenuation
            half3 ambient = _Ka * s.Albedo;
            half3 lit     = _Kd * diff * s.Albedo * _LightColor0.rgb * atten
                          + _Ks * spec            * _LightColor0.rgb * atten;

            return half4(ambient + lit, 1);
        }
        ENDCG
    }

    // Fallback for very old paths
    Fallback "Diffuse"
}
