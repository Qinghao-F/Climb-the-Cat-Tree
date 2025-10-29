//UNITY_SHADER_NO_UPGRADE

Shader "Unlit/WaveShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			uniform sampler2D _MainTex;
			uniform float4x4 _CustomMVP; // For task 9 (challenge) - see corresponding CustomMVP.cs file

			struct vertIn
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct vertOut
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// Implementation of the vertex shader
			vertOut vert(vertIn v)
			{
				// Displace the original vertex in model space
				//float4 displacement = float4(0.0f, 0.0f, 0.0f, 0.0f);
				//float4 displacement = float4(0.0f, 5.0f, 0.0f, 0.0f); // Task 2a
				//float4 displacement = float4(0.0f, _Time.y, 0.0f, 0.0f); // Task 2b
				//float4 displacement = float4(0.0f, sin(_Time.y), 0.0f, 0.0f); // Task 2c
				//float4 displacement = float4(0.0f, sin(v.vertex.x), 0.0f, 0.0f); // Task 3
				//float4 displacement = float4(0.0f, sin(v.vertex.x + _Time.y), 0.0f, 0.0f); // Task 4
				//float4 displacement = float4(0.0f, sin(v.vertex.x + _Time.y) * 0.5f, 0.0f, 0.0f); // Task 5a
				//float4 displacement = float4(0.0f, sin(v.vertex.x + _Time.y * 2.0f), 0.0f, 0.0f); // Task 5b
				float4 displacement = float4(0.0f, sin(v.vertex.x + _Time.y * _Time.y), 0.0f, 0.0f); // Task 5c
				v.vertex += displacement;

				vertOut o;

				// Tasks 6 onwards get you to work with MVP...
				// Uncomment the respective sections to see them in action.
				// The solution for Task 8 has been uncommented by default.
				
				//o.vertex = mul(UNITY_MATRIX_MVP, v.vertex); // Default

				// Task 6 - remove M
				//o.vertex = mul(UNITY_MATRIX_VP, v.vertex); 

				// Task 7 - remove V - Unfortunately there is no UNITY_MATRIX_MP
				// so we have to do this ourselves.
				//o.vertex = mul(mul(UNITY_MATRIX_P, unity_ObjectToWorld), v.vertex); 
				
				// Task 8 - Need to apply wave transformation between MV and P!
				// Apply the model and view matrix to the vertex (but not the projection matrix yet)
				//v.vertex = mul(UNITY_MATRIX_MV, v.vertex);

				// v.vertex is now in view space. This is the point where we want to apply the displacement.
				//v.vertex += float4(0.0f, sin(v.vertex.x), 0.0f, 0.0f);
				
				// Finally apply the projection matrix to complete the transformation into screen space
				//o.vertex = mul(UNITY_MATRIX_P, v.vertex);

				// Task 9 (challenge)
				// Check out the CustomMVP.cs script as well...
				// Obviously there's no need to ever do this yourself, as Unity
				// already gives us UNITY_MATRIX_MVP, but it's a good learning
				// exercise to see what's going on under the hood.
				o.vertex = mul(_CustomMVP, v.vertex); 

				o.uv = v.uv;
				return o;
			}
			
			// Implementation of the fragment shader
			fixed4 frag(vertOut v) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, v.uv);
				return col;
			}
			ENDCG
		}
	}
}
