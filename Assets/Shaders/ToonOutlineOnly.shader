Shader "Toon/OutlineOnly"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0.10,0.10,0.10,1) // near-black toon ink
        _OutlineWidth ("Outline Width (px)", Range(0.5,8)) = 2.8    // tuned to match mouse outline
        _ZOffset      ("Depth Offset", Range(-2,2)) = 0.12           // reduces z-fighting
    }

    SubShader
    {
        Tags { "Queue"="Geometry+10" "RenderType"="Opaque" }
        Cull Front                  // back faces form the expanded shell
        ZWrite Off                  // do not write depth; keep base mesh visible
        ZTest LEqual
        Offset 0, [_ZOffset]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor;
            float  _OutlineWidth;   // width in screen pixels

            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
            struct v2f      { float4 pos:SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;

                // Base clip position
                float4 pos = UnityObjectToClipPos(v.vertex);

                // View-space normal → stable screen-space direction
                float3 nWorld = UnityObjectToWorldNormal(v.normal);
                float3 nView  = mul((float3x3)UNITY_MATRIX_V, nWorld);

                // Constant-pixel offset (independent of distance)
                float2 dir = normalize(nView.xy + 1e-4);                  // avoid NaN on flat normals
                float2 ndcPerPixel = 1.0 / _ScreenParams.xy;              // NDC per pixel
                float2 offsetNDC   = dir * (_OutlineWidth * ndcPerPixel) * pos.w;
                pos.xy += offsetNDC;

                o.pos = pos;
                return o;
            }

            fixed4 frag () : SV_Target
            {
                return _OutlineColor; // flat ink colour
            }
            ENDCG
        }
    }
    Fallback Off
}
