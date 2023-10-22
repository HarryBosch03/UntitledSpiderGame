Shader "Unlit/Player"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Index ("Index", int) = 0
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
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float2 uv0 : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.uv0 = input.uv;
				return output;
			}

			CBUFFER_START(UnityPerMaterial)
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			float4 _MainTex_TexelSize;
			int _Index;
			CBUFFER_END
			
			half4 frag (Varyings input) : SV_Target
			{
				half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(_Index * _MainTex_TexelSize.x, input.uv0.y));
				return col;
			}
			ENDHLSL
		}
	}
}