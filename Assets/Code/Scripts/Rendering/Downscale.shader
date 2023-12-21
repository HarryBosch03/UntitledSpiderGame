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
                float2 uv : TEXCOORD0;
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
                output.uv = output.vertex * 0.5 + 0.5;
                output.uv.y = 1.0 - output.uv.y;
                return output;
            }

            static const float ppu = 8.0f;

            float4 _CameraOpaqueTexture_TexelSize;
            
            half4 frag(Varyings input) : SV_Target
            {
                float pixels = unity_OrthoParams.y * ppu;
                float aspect = _CameraOpaqueTexture_TexelSize.z * _CameraOpaqueTexture_TexelSize.y;
                float2 downscale = float2(aspect, 1.0) * pixels;

                float2 uv = floor(input.uv * downscale) / downscale;
                half3 col = SampleSceneColor(uv);
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}