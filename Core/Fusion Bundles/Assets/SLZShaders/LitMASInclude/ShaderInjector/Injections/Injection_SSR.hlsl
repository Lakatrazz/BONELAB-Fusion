//#!INJECT_BEGIN STANDALONE_DEFINES 0
#pragma multi_compile _ _SLZ_SSR_ENABLED
#pragma shader_feature_local _ _NO_SSR
#if defined(_SLZ_SSR_ENABLED) && !defined(_NO_SSR) && !defined(SHADER_API_MOBILE)
    #define _SSR_ENABLED
#endif
//#!INJECT_END

//#!INJECT_BEGIN INCLUDES 0
#if !defined(SHADER_API_MOBILE)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SLZLightingSSR.hlsl"
#endif
//#!INJECT_END

//#!INJECT_BEGIN INTERPOLATORS 1
    //#!TEXCOORD float4 lastVertex 1
//#!INJECT_END

//#!INJECT_BEGIN VERTEX_END 0
    #if defined(_SSR_ENABLED)
        float4 lastWPos = mul(GetPrevObjectToWorldMatrix(), v.vertex);
        o.lastVertex = mul(prevVP, lastWPos);
    #endif
//#!INJECT_END

//#!INJECT_BEGIN LIGHTING_CALC 0
	#if defined(_SSR_ENABLED)
        half4 noiseRGBA = GetScreenNoiseRGBA(fragData.screenUV);

        SSRExtraData ssrExtra;
        ssrExtra.meshNormal = i.normXYZ_tanX.xyz;
        ssrExtra.lastClipPos = i.lastVertex;
        ssrExtra.temporalWeight = _SSRTemporalMul;
        ssrExtra.depthDerivativeSum = 0;
        ssrExtra.noise = noiseRGBA;
        ssrExtra.fogFactor = i.uv0XY_bitZ_fog.w;

        color.rgb = max(0,SLZPBRFragmentSSR(fragData, surfData, ssrExtra));
    #else
        color.rgb = SLZPBRFragment(fragData, surfData);
    #endif
//#!INJECT_END

//#!INJECT_BEGIN VOLUMETRIC_FOG 0
    #if !defined(_SSR_ENABLED)
        color.rgb = MixFog(color.rgb, -fragData.viewDir, i.uv0XY_bitZ_fog.w);
        color = Volumetrics(color, fragData.position);
    #endif
//#!INJECT_END