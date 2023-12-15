Shader "Unlit/Player"
{
	Properties
	{
		_MainColor("Main Color", Color) = (1.0, 0.4, 0.1, 1.0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		Cull Off

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

			struct Attributes
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct Varyings
			{
				float4 color : COLOR;
				float4 vertex : SV_POSITION;
			};

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.color = input.color;
				return output;
			}

			CBUFFER_START(UnityPerMaterial)
			float4 _MainColor;
			float4 _Tint;
			CBUFFER_END
			
			half4 frag (Varyings input) : SV_Target
			{
				return _MainColor * input.color;
			}
			ENDHLSL
		}
	}
}