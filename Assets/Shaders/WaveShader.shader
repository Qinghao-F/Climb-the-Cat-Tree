// UNITY_SHADER_NO_UPGRADE
Shader "Unlit/WaveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        // 新增参数
        _MaxAmp ("Max Amplitude", Range(0,5)) = 2     // 最远端的最大振幅
        _Freq   ("Wave Frequency", Float)      = 1     // 波的空间频率
        _Speed  ("Wave Speed", Float)          = 1     // 波的时间速度
        _Falloff("Edge Falloff (pow)", Range(0,4)) = 1 // 距杆衰减的陡峭程度
        _PoleOnRight ("Pole On Right? (0=left,1=right)", Float) = 0 // 旗杆在右边就设为1
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
                // 基于UV的衰减：靠近旗杆侧振幅更小
                // 假设旗杆在UV.x=0一侧；若在右侧，使用 _PoleOnRight 反转
                float u = v.uv.x;
                // 当 _PoleOnRight >= 0.5 时，把 u 反转为 (1-u)
                u = lerp(u, 1.0 - u, step(0.5, _PoleOnRight));

                // 衰减曲线：pow(u, _Falloff)（_Falloff 越大，靠杆越“硬”）
                float attn = pow(saturate(1.0 - u), _Falloff);

                // 最终振幅（最远端为 _MaxAmp，靠近旗杆趋近 0）
                float amp = _MaxAmp * attn;

                // 正弦位移（你原来的写法 + 可调速度/频率）
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

