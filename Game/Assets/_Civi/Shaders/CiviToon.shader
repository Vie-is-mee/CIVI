// Single material for static world geometry. Colours live in vertex colours so a whole level is one draw setup.
Shader "Civi/Toon"
{
    Properties
    {
        _BaseMap ("Base", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
        _ShadowTint ("Shadow tint", Color) = (1,.96,1,1)
        _ShadeStep ("Shade step", Range(-1,1)) = 0.02
        _ShadeSoft ("Shade softness", Range(0,1)) = 0.06
        _RimColor ("Rim", Color) = (1,1,1,0)
        _RimPower ("Rim power", Range(1,12)) = 5
        _Emission ("Emission", Float) = 2.4
        _OutlineColor ("Outline", Color) = (0,0,0,1)
        _OutlineWidth ("Outline", Float) = 0
        _Cutoff ("Cutoff", Range(0,1)) = .5
        _LensExempt ("Lens exempt", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ToonVert
            #pragma fragment ToonFrag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "CiviToon.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "CiviToon.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #include "CiviToon.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}
