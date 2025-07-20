Shader "Unlit/Rainbow"
{
     Properties
    {
        _Speed ("Color Change Speed", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Speed;
            CBUFFER_END

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // 시간 기반으로 색상 변화를 위한 값 계산
                float t = _Time.y * _Speed;
                
                // UV 좌표를 사용하여 층별 무지개 생성
                // Y 좌표(0~1)를 기반으로 무지개 색상 계산
                float hue = input.uv.y + t * 0.1; // 시간에 따라 천천히 회전
                
                // HSV to RGB 변환을 통한 무지개 색상 생성
                float3 rainbowColor;
                float h = frac(hue) * 6.0; // 0~6 범위의 색상 값
                float c = 1.0; // 채도
                float x = c * (1.0 - abs(fmod(h, 2.0) - 1.0));
                
                if (h < 1.0) {
                    rainbowColor = float3(c, x, 0.0);
                } else if (h < 2.0) {
                    rainbowColor = float3(x, c, 0.0);
                } else if (h < 3.0) {
                    rainbowColor = float3(0.0, c, x);
                } else if (h < 4.0) {
                    rainbowColor = float3(0.0, x, c);
                } else if (h < 5.0) {
                    rainbowColor = float3(x, 0.0, c);
                } else {
                    rainbowColor = float3(c, 0.0, x);
                }

                // 색상에 포그 효과 적용
                half3 finalColor = MixFog(rainbowColor, input.fogFactor);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
