// Dome sky: gradient, sun glow and drifting clouds; cheap enough for every pixel of the title flyover.
Shader "Civi/Sky"
{
    Properties
    {
        _Top ("Top", Color) = (.2,.55,.9,1)
        _Horizon ("Horizon", Color) = (.82,.93,.98,1)
        _Bottom ("Bottom", Color) = (.5,.7,.8,1)
        _Sun ("Sun glow", Color) = (1,.92,.75,1)
        _SunDir ("Sun direction", Vector) = (.3,.6,.7,0)
        _Cloud ("Cloud", Color) = (1,1,1,.8)
        _Stars ("Stars", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Top, _Horizon, _Bottom, _Sun, _Cloud;
                float4 _SunDir;
                half _Stars;
            CBUFFER_END
            struct A { float4 p : POSITION; };
            struct V { float4 p : SV_POSITION; float3 d : TEXCOORD0; };
            V Vert(A i) { V o; o.p = TransformObjectToHClip(i.p.xyz); o.d = i.p.xyz; return o; }
            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float Noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3 - 2 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), f.x), lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), f.x), f.y);
            }
            half4 Frag(V i) : SV_Target
            {
                float3 d = normalize(i.d);
                half3 col = d.y > 0 ? lerp(_Horizon.rgb, _Top.rgb, pow(saturate(d.y), .55)) : lerp(_Horizon.rgb, _Bottom.rgb, saturate(-d.y * 6));
                float sun = saturate(dot(d, normalize(_SunDir.xyz)));
                col += _Sun.rgb * (pow(sun, 8) * .28 + pow(sun, 220) * 1.6);
                if (d.y > .01)
                {
                    float2 uv = d.xz / (d.y + .18) * 1.6 + _Time.x * float2(.5, .18);
                    float n = Noise(uv) * .55 + Noise(uv * 2.1) * .3 + Noise(uv * 4.3) * .15;
                    col = lerp(col, _Cloud.rgb, smoothstep(.52, .78, n) * _Cloud.a * saturate(d.y * 5));
                    if (_Stars > 0)
                    {
                        float s = Hash(floor(d.xz / (d.y + .3) * 170));
                        col += step(.992, s) * _Stars * saturate(d.y * 4) * .8;
                    }
                }
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
