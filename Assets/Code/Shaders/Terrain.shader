Shader "Terrain/Unlit/Terrain"
{
	Properties
	{

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

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
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};
			
			TEXTURE2D(_MapWeights);
			SAMPLER(sampler_MapWeights);
			int _MapWidth;
			int _MapHeight;
			
			Varyings vert (Attributes input)
			{
				Varyings output;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.color = input.color;
				return output;
			}
			
			half4 frag (Varyings input) : SV_Target
			{
				return float4(pow(input.color.rgb, 2.2), input.color.a);
			}
			ENDHLSL
		}
	}
}