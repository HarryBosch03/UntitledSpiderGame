Shader "Hidden/Downscale"
{
    Properties { }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 position : VAR_POSITION;
            };

            static const float2 verts[] =
            {
                float2(-1, -1),
                float2(4, -1),
                float2(-1, 4),
            };

            Varyings vert(uint id : SV_VertexID)
            {
                Varyings output;
                output.vertex = float4(verts[id], 0, 1);
                output.position = mul(UNITY_MATRIX_I_VP, output.vertex).xy;
                return output;
            }

            static const float ppu = 16.0f;

            float4 _CameraOpaqueTexture_TexelSize;
            
            half4 frag(Varyings input) : SV_Target
            {
                half2 worldPos = floor(input.position * ppu) / ppu;
                half2 clipPos = TransformWorldToHClip(float3(worldPos, 0)).xy;
                half2 uv = (clipPos + 1.0) * 0.5;
                uv.y = 1 - uv.y;

                half3 col = SampleSceneColor(uv);
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}