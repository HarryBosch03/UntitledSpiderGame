Shader "Unlit/Flipbook"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HFrames("Horizontal Frames", int) = 1
		_VFrames("Vertical Frames", int) = 1
		_Framerate("Framerate", int) = 18
		_Brightness("Brightness", float) = 0
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
			};

			struct Varyings
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.uv = input.uv;
				return output;
			}

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			int _HFrames;
			int _VFrames;
			int _Framerate;

			float _Brightness;
			
			half4 frag (Varyings input) : SV_Target
			{
				float2 uv = input.uv / float2(_HFrames, _VFrames);
				int i = floor(_Time.y * _Framerate) % (_HFrames * _VFrames);
				uv.x += (float)i / _HFrames;
				uv.y += (float)i / (_HFrames * _VFrames);
				
				half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
				return col * pow(2, _Brightness);
			}
			ENDHLSL
		}
	}
}