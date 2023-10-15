Shader "Unlit/Lava"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TopColor("Top Color", Color) = (1, 1, 1, 1)
        _Brightness("Brightness", float) = 0.0

        _WaveHeight("Wave Height", float) = 3
        _WaveSize("Wave Size", float) = 10.0
        _WaveSpeed("Wave Speed", float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _WaveHeight;
            float _WaveSize;
            float _WaveSpeed;
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 offset = float3(0.0, _WaveHeight / 16.0, 0.0);
                float3 worldPos = TransformObjectToWorld(input.vertex.xyz) +  + offset;
                output.uv = worldPos.xy - TransformObjectToWorld(float3(-0.5, 0.5, 0.0)).xy;
                output.vertex = TransformWorldToHClip(worldPos);
                return output;
            }

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _TopColor;
            float _Brightness;

            float WaveLayer(float x, float depth)
            {
                return sin(x / _WaveSize * depth + _Time.y * _WaveSpeed / depth) * _WaveHeight / 16.0 / depth;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = float2(input.uv.x, -input.uv.y);
                uv = floor(uv * 16) / 16;

                float wave = 0.0f;
                wave += WaveLayer(uv.x, 2);
                wave += WaveLayer(uv.x, -4);
                wave += WaveLayer(uv.x, 8);
                wave += WaveLayer(uv.x, -16);

                uv.y += pow(wave * 0.5 + 0.5, 2) * 2.0 - 1.0;
                
                clip(uv.y);
                if (uv.y < 1.0 / 16.0)
                {
                    return _TopColor;
                }

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(uv.x, 1.0f / (1.0f + uv.y)));
                return col * pow(2, _Brightness);
            }
            ENDHLSL
        }
    }
}