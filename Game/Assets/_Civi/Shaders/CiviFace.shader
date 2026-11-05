// Anime face decal. _FaceUV selects one expression cell in the atlas per renderer.
Shader "Civi/Face"
{
    Properties
    {
        _BaseMap ("Face atlas", 2D) = "white" {}
        _FaceUV ("Cell scale/offset", Vector) = (.25,.5,0,.5)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry+10" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _FaceUV;
            CBUFFER_END
            struct A { float4 p : POSITION; float3 n : NORMAL; float2 uv : TEXCOORD0; };
            struct V { float4 p : SV_POSITION; float2 uv : TEXCOORD0; half3 light : TEXCOORD1; };
            V Vert(A i)
            {
                V o;
                o.p = TransformObjectToHClip(i.p.xyz);
                o.uv = i.uv * _FaceUV.xy + _FaceUV.zw;
                // A face stays readable: it follows the light level, never a hard shadow line.
                o.light = saturate(SampleSH(TransformObjectToWorldNormal(i.n)) + GetMainLight().color * .55);
                return o;
            }
            half4 Frag(V i) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                c.rgb *= i.light;
                return c;
            }
            ENDHLSL
        }
    }
}
