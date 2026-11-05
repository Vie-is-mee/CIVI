#ifndef CIVI_TOON_INCLUDED
#define CIVI_TOON_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _ShadowTint;
    half4 _RimColor;
    half4 _OutlineColor;
    half _Cutoff;
    half _ShadeStep;
    half _ShadeSoft;
    half _RimPower;
    half _Emission;
    float _OutlineWidth;
    half _LensExempt;
CBUFFER_END

// Set by the Debug lens: 0 = normal view, 1 = data view.
float _CiviLens;


struct ToonAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct ToonVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
    half4 color : COLOR;
    half3 sh : TEXCOORD3;
    half fog : TEXCOORD4;
    UNITY_VERTEX_OUTPUT_STEREO
};

ToonVaryings ToonVert(ToonAttributes v)
{
    ToonVaryings o = (ToonVaryings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
    o.positionCS = p.positionCS;
    o.positionWS = p.positionWS;
    o.normalWS = TransformObjectToWorldNormal(v.normalOS);
    o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
    // Blender exports sRGB bytes; alpha 1 is a plain surface, lower alpha adds self-glow.
    o.color = half4(SRGBToLinear(v.color.rgb), 1 - v.color.a);
    o.sh = SampleSH(o.normalWS);
    o.fog = ComputeFogFactor(p.positionCS.z);
    return o;
}

half3 LensView(half3 col, float3 positionWS)
{
    if (_CiviLens <= 0) return col;
    half luma = dot(col, half3(.3, .59, .11));
    float2 cell = abs(frac(positionWS.xz * .5) - .5);
    half grid = step(.47, max(cell.x, cell.y));
    half3 view = half3(.02, .16, .19) + half3(.05, .55, .5) * luma + half3(.1, .55, .48) * grid * .35;
    return lerp(col, view, _CiviLens * (1 - _LensExempt));
}

half3 ToonLight(half3 albedo, half glow, float3 positionWS, half3 normalWS, half3 sh)
{
    half3 N = normalize(normalWS);
    Light main = GetMainLight(TransformWorldToShadowCoord(positionWS), positionWS, half4(1, 1, 1, 1));
    half ndl = dot(N, main.direction);
    half lit = smoothstep(_ShadeStep - _ShadeSoft, _ShadeStep + _ShadeSoft, ndl) * main.shadowAttenuation;
    half3 V = normalize(GetWorldSpaceViewDir(positionWS));
    half rim = pow(1 - saturate(dot(N, V)), _RimPower) * (.35 + .65 * lit);
    half3 col = albedo * (sh * _ShadowTint.rgb + main.color * lit) + _RimColor.rgb * rim * _RimColor.a;
#if defined(_ADDITIONAL_LIGHTS)
    uint count = GetAdditionalLightsCount();
    for (uint i = 0u; i < count; ++i)
    {
        Light l = GetAdditionalLight(i, positionWS, half4(1, 1, 1, 1));
        half wrap = saturate(dot(N, l.direction) * .6 + .4);
        col += albedo * l.color * l.distanceAttenuation * wrap;
    }
#endif
    return col + albedo * glow * _Emission;
}

half4 ToonFrag(ToonVaryings i) : SV_Target
{
    half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
    half3 col = ToonLight(tex.rgb * _BaseColor.rgb * i.color.rgb, i.color.a, i.positionWS, i.normalWS, i.sh);
    col = LensView(col, i.positionWS);
    return half4(MixFog(col, i.fog), 1);
}

struct OutlineVaryings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    half fog : TEXCOORD1;
    UNITY_VERTEX_OUTPUT_STEREO
};

OutlineVaryings OutlineVert(ToonAttributes v)
{
    OutlineVaryings o = (OutlineVaryings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
    float3 nCS = mul((float3x3)UNITY_MATRIX_VP, TransformObjectToWorldNormal(v.normalOS));
    // Steady on-screen thickness at any camera distance, up to a limit for close-ups.
    float2 offset = normalize(nCS.xy + 1e-5) * _OutlineWidth * min(p.positionCS.w, 2.5);
    offset.x *= _ScreenParams.y / _ScreenParams.x;
    o.positionCS = p.positionCS;
    o.positionCS.xy += offset;
    o.positionWS = p.positionWS;
    o.fog = ComputeFogFactor(p.positionCS.z);
    return o;
}

half4 OutlineFrag(OutlineVaryings i) : SV_Target
{
    return half4(MixFog(LensView(_OutlineColor.rgb, i.positionWS), i.fog), 1);
}
#endif
