Shader "Unlit/WaveShader_WebGLSafe"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaxAmp ("Max Amplitude", Range(0,5)) = 2
        _Freq   ("Wave Frequency", Float) = 1
        _Speed  ("Wave Speed", Float) = 1
        _Falloff("Edge Falloff (pow)", Range(0,4)) = 1
        _PoleOnRight ("Pole On Right? (0=left,1=right)", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _MaxAmp, _Freq, _Speed, _Falloff, _PoleOnRight;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                float u = v.uv.x;
                u = lerp(u, 1.0 - u, step(0.5, _PoleOnRight));

                float attn = pow(saturate(1.0 - u), _Falloff);
                float amp = _MaxAmp * attn;
                float wave = sin(v.vertex.x * _Freq + _Time.y * _Speed);
                v.vertex.y += amp * wave;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
