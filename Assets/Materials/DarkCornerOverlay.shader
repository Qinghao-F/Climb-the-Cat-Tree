// UNITY_SHADER_NO_UPGRADE
Shader "Unlit/DarkCornerOverlay_Driven"
{
    Properties
    {
        _MainTex   ("Vignette PNG (with alpha)", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _MaxAlpha  ("Max Alpha", Range(0,1)) = 1
        _Progress  ("Progress 0..1 (from script)", Range(0,1)) = 0
        _UseTexAlpha ("Use Texture Alpha (0/1)", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex; float4 _MainTex_ST;
            fixed4 _TintColor;
            float _MaxAlpha, _Progress, _UseTexAlpha;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f     { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert (appdata v){
                v2f o; o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i):SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 c = tex * _TintColor;

                // driven by script
                float t = saturate(_Progress);

                // Whether to multiply by the texture's alpha
                float baseA = lerp(1.0, tex.a, saturate(_UseTexAlpha));

                // from transparent (0) to MaxAlpha * baseA
                c.a = t * _MaxAlpha * baseA;
                return c;
            }
            ENDCG
        }
    }
}
