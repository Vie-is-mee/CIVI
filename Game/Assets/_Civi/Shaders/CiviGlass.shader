// Windows and the dome shell: fresnel tinted glass with no refraction pass, so it stays cheap.
Shader "Civi/Glass"
{
    Properties
    {
        _Color ("Tint", Color) = (.72,.92,1,.08)
        _Edge ("Edge", Color) = (.8,1,1,.45)
        _Power ("Edge power", Range(.5,8)) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color, _Edge;
                half _Power;
            CBUFFER_END
            struct A { float4 p : POSITION; float3 n : NORMAL; };
            struct V { float4 p : SV_POSITION; float3 n : TEXCOORD0; float3 w : TEXCOORD1; };
            V Vert(A i)
            {
                V o;
                o.w = TransformObjectToWorld(i.p.xyz);
                o.p = TransformWorldToHClip(o.w);
                o.n = TransformObjectToWorldNormal(i.n);
                return o;
            }
            half4 Frag(V i) : SV_Target
            {
                float3 v = normalize(GetWorldSpaceViewDir(i.w));
                half f = pow(1 - abs(dot(normalize(i.n), v)), _Power);
                return half4(lerp(_Color.rgb, _Edge.rgb, f), lerp(_Color.a, _Edge.a, f));
            }
            ENDHLSL
        }
    }
}
