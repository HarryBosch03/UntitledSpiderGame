Shader "Crabs/Unlit/Color"
{
	Properties
	{
		[MainColor] _Color("Color", Color) = (1, 1, 1, 1)
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
			
			float4 vert (float4 vertex : POSITION) : SV_POSITION
			{
				return TransformObjectToHClip(vertex.xyz);
			}

			float4 _Color;
			
			half4 frag (float4 vertex : SV_POSITION) : SV_Target
			{
				return _Color;
			}
			ENDHLSL
		}
	}
}