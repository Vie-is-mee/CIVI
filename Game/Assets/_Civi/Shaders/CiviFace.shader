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
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
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
                // A face stays readable: it follows the light level, never a hard shadow line. Room lamps count too,
                // or eyes go black in a lit room at night while the skin around them glows.
                half3 n = TransformObjectToWorldNormal(i.n);
                half3 light = SampleSH(n) + GetMainLight().color * .55;
            #if defined(_ADDITIONAL_LIGHTS)
                float3 ws = TransformObjectToWorld(i.p.xyz);
                uint count = GetAdditionalLightsCount();
                for (uint k = 0u; k < count; ++k)
                {
                    Light l = GetAdditionalLight(k, ws, half4(1, 1, 1, 1));
                    light += l.color * l.distanceAttenuation * saturate(dot(n, l.direction) * .6 + .4);
                }
            #endif
                o.light = saturate(light);
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
