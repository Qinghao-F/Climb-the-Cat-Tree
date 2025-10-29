// UNITY_SHADER_NO_UPGRADE
Shader "Unlit/WaveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _MaxAmp ("Max Amplitude", Range(0,5)) = 2     // Maximum amplitude at the far end
        _Freq   ("Wave Frequency", Float)      = 1     // Spatial frequency of the wave
        _Speed  ("Wave Speed", Float)          = 1     // Temporal speed of the wave (animation speed)
        _Falloff("Edge Falloff (pow)", Range(0,4)) = 1 // Steepness of the falloff from the pole
        _PoleOnRight ("Pole On Right? (0=left,1=right)", Float) = 0
    }
    SubShader
    {
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4x4  _CustomMVP;

            float _MaxAmp, _Freq, _Speed, _Falloff, _PoleOnRight;

            struct vertIn {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct vertOut {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            vertOut vert(vertIn v)
            {
                // UV-based falloff: smaller amplitude near the pole side
                float u = v.uv.x;
                u = lerp(u, 1.0 - u, step(0.5, _PoleOnRight));

                // Falloff curve
                float attn = pow(saturate(1.0 - u), _Falloff);

                // Final amplitude
                float amp = _MaxAmp * attn;

                // Sine displacement
                float wave = sin(v.vertex.x * _Freq + _Time.y * _Speed);

                v.vertex.y += amp * wave;

                vertOut o;
                o.vertex = mul(_CustomMVP, v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(vertOut i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}

