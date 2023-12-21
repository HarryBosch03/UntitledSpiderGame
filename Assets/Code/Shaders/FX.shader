Shader "Unlit/FX"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Brightness("Brightness", float) = 1.0
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
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct Varyings
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.uv = input.uv;
				output.color = input.color;
				return output;
			}

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			float _Brightness;
			
			half4 frag (Varyings input) : SV_Target
			{
				half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
				col.rgb *= _Brightness;
				clip(col.a - 0.1f);
				return col;
			}
			ENDHLSL
		}
	}
}