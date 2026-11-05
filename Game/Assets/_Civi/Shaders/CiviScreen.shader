// Holographic desk screens: scrolling data bands drawn in the shader, no textures or video.
Shader "Civi/Screen"
{
    Properties
    {
        _Color ("Glow", Color) = (.25,.95,.86,1)
        _Dark ("Back", Color) = (.02,.13,.16,1)
        _Speed ("Speed", Float) = .35
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color, _Dark;
                half _Speed;
            CBUFFER_END
            float _CiviLens;
            struct A { float4 p : POSITION; float2 uv : TEXCOORD0; };
            struct V { float4 p : SV_POSITION; float2 uv : TEXCOORD0; half fog : TEXCOORD1; };
            V Vert(A i)
            {
                V o;
                VertexPositionInputs p = GetVertexPositionInputs(i.p.xyz);
                o.p = p.positionCS;
                o.uv = i.uv;
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }
            float Hash(float x) { return frac(sin(x * 91.17) * 43758.5453); }
            half4 Frag(V i) : SV_Target
            {
                float2 uv = i.uv;
                float rows = floor(uv.y * 14 + _Time.y * _Speed);
                float bar = step(frac(uv.x * 3.2 + Hash(rows)), .25 + Hash(rows + 7) * .7) * step(frac(uv.y * 14 + _Time.y * _Speed), .55);
                float grid = step(.965, frac(uv.x * 18)) + step(.95, frac(uv.y * 10));
                float wave = 1 - smoothstep(0, .025, abs(uv.y - .22 - sin(uv.x * 11 + _Time.y) * .07));
                half3 col = lerp(_Dark.rgb, _Color.rgb, saturate(bar * .55 + grid * .12 + wave)) * (1 + _CiviLens);
                return half4(MixFog(col * 1.5, i.fog), 1);
            }
            ENDHLSL
        }
    }
}
