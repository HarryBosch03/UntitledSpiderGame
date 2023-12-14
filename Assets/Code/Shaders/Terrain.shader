Shader "Terrain/Unlit/Terrain"
{
	Properties
	{
		_AccentColor ("Moss Color", Color) = (1, 0, 0, 1)
		_ColorHigh ("Color High", Color) = (1, 1, 1, 1)
		_ColorLow ("Color Low", Color) = (0.5, 0.5, 0.5, 1)
		[HideInInspector] _Debug("Debug Color", Color) = (1, 1, 1, 0)
		[Toggle] _ShowDebug("Show Debug", float) = 0
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
				float2 uv0 : TEXCOORD0;
				float depth : VAR_DEPTH;
				float4 vertex : SV_POSITION;
				float2 uv1 : TILE_INDEX;
			};
			
			TEXTURE2D(_MapWeights);
			SAMPLER(sampler_MapWeights);
			int _MapWidth;
			int _MapHeight;
			
			Varyings vert (Attributes input)
			{
				Varyings output;
				output.depth = input.vertex.z;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.uv0 = input.uv;
				output.uv1 = input.color.rg;
				return output;
			}

			CBUFFER_START(UnityPerMaterial)
			float4 _ColorLow;
			float4 _ColorHigh;
			float4 _AccentColor;
			float4 _Debug;
			float _ShowDebug;
			CBUFFER_END
			
			half4 frag (Varyings input) : SV_Target
			{
				half4 sample = SAMPLE_TEXTURE2D(_MapWeights, sampler_MapWeights, input.uv1);

				half3 color = lerp(_ColorLow, _ColorHigh, saturate(exp(30 * -(sample * sample)))).rgb;
				color = _ColorHigh.rgb;

				color.rgb = lerp(color, _Debug.rgb, _Debug.a * _ShowDebug);
				
				return float4(color, 1.0);
			}
			ENDHLSL
		}
	}
}