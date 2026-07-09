Shader "Custom/DepthAPI/TransparentOcclusion"
{
    Properties
    {
        _BaseMap("Texture",2D)="white"{}
        _BaseColor("Color",Color)=(1,1,1,1)
        _Opacity("Opacity",Range(0,1))=1
        _EnvironmentDepthBias("Depth Bias",Float)=0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Cull Back
            ZWrite Off

            // Premultiplied Alpha
            Blend One OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

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
            half _Opacity;
            float _EnvironmentDepthBias;

            Varyings vert(Attributes input)
            {
                Varyings o;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs pos =
                    GetVertexPositionInputs(input.positionOS.xyz);

                o.positionCS = pos.positionCS;
                o.uv = TRANSFORM_TEX(input.uv,_BaseMap);

                META_DEPTH_INITIALIZE_VERTEX_OUTPUT(o, input.positionOS);

                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 col =
                    SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv)
                    * _BaseColor;

                col.a *= _Opacity;

                // Premultiply
                col.rgb *= col.a;

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