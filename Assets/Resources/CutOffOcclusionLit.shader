Shader "Custom/CutOffOcclusionLit"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _EnvironmentDepthBias("Depth Bias", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }

        Pass
        {
            Name "Forward"

            Cull Back
            ZWrite On
            Blend One Zero

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // Depth API
            #pragma multi_compile _ HARD_OCCLUSION SOFT_OCCLUSION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.meta.xr.sdk.core/Shaders/EnvironmentDepth/URP/EnvironmentOcclusionURP.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;

                META_DEPTH_VERTEX_OUTPUT(1)

                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Cutoff;
            float _EnvironmentDepthBias;

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs pos =
                    GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = pos.positionCS;
                output.uv = TRANSFORM_TEX(input.uv,_BaseMap);

                META_DEPTH_INITIALIZE_VERTEX_OUTPUT(output, input.positionOS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 col =
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv)
                    * _BaseColor;

                // Alpha Cutout
                clip(col.a - _Cutoff);

                // Meta Depth API Occlusion
                META_DEPTH_OCCLUDE_OUTPUT_PREMULTIPLY(
                    input,
                    col,
                    _EnvironmentDepthBias
                );

                return col;
            }

            ENDHLSL
        }
    }
}