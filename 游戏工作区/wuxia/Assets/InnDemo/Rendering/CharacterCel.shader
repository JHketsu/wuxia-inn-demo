Shader "Wuxia/Character Cel Prototype"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _ShadeTint ("Shade Tint", Color) = (0.67,0.68,0.76,1)
        _Threshold ("Light Threshold", Range(-1,1)) = 0.25
        _TransitionWidth ("Transition Width", Range(0.01,0.4)) = 0.10
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "CelForward"
            Tags { "LightMode"="UniversalForwardOnly" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "CharacterCelLighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadeTint;
                half _Threshold;
                half _TransitionWidth;
                float _Cull;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half fog : TEXCOORD3;
            #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                float4 screenPosition : TEXCOORD4;
            #endif
            };

            Varyings Vert(Attributes input)
            {
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                Varyings output;
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fog = ComputeFogFactor(position.positionCS.z);
            #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                output.screenPosition = ComputeScreenPos(position.positionCS);
            #endif
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
            #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                Light light = GetMainLight(input.screenPosition);
            #else
                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
            #endif
                half3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half ndotl = dot(normalize(input.normalWS), light.direction);
                half3 lighting = CharacterCelLighting(ndotl, light.shadowAttenuation * light.distanceAttenuation,
                    light.color, _ShadeTint.rgb, _Threshold, _TransitionWidth);
                return half4(MixFog(baseColor * lighting, input.fog), 1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
