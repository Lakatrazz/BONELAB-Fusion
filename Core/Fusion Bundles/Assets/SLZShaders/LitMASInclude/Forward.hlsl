
#define SHADERPASS SHADERPASS_FORWARD
#define _NORMAL_DROPOFF_TS 1
#define _EMISSION
#define _NORMALMAP 1

#if defined(SHADER_API_MOBILE)
    #define _ADDITIONAL_LIGHTS_VERTEX
#else              
    #pragma multi_compile           _   _MAIN_LIGHT_SHADOWS_CASCADE
    #pragma multi_compile_fragment  _   _SCREEN_SPACE_OCCLUSION
    
    #pragma shader_feature_fragment _ADDITIONAL_LIGHTS
    #pragma shader_feature_fragment _ADDITIONAL_LIGHT_SHADOWS
    #pragma shader_feature_fragment _SHADOWS_SOFT
    
    #pragma shader_feature_fragment _REFLECTION_PROBE_BLENDING
    //#pragma shader_feature_fragment _REFLECTION_PROBE_BOX_PROJECTION
    // We don't need a keyword for this! the w component of the probe position already branches box vs non-box, & so little cost on pc it doesn't matter
    #define _REFLECTION_PROBE_BOX_PROJECTION 

    #if defined(LITMAS_FEATURE_SSR)
        #pragma shader_feature_local_fragment _ _SSR_ON
    #endif
#endif

#if defined(LITMAS_FEATURE_TP)
    #pragma multi_compile_local_fragment _ _EXPENSIVE_TP

    #if defined(_EXPENSIVE_TP)
        #define SLZ_SAMPLE_TP_MAIN(tex, sampl, uv) SAMPLE_TEXTURE2D_GRAD(tex, sampl, uv, ddxMain, ddyMain)
        #define SLZ_SAMPLE_TP_DETAIL(tex, sampl, uv) SAMPLE_TEXTURE2D_GRAD(tex, sampl, uv, ddxDetail, ddyDetail)
    #else
        #define SLZ_SAMPLE_TP_MAIN(tex, sampl, uv) SAMPLE_TEXTURE2D(tex, sampl, uv)
        #define SLZ_SAMPLE_TP_DETAIL(tex, sampl, uv) SAMPLE_TEXTURE2D(tex, sampl, uv)
    #endif
#endif

#pragma multi_compile_fragment _ _LIGHT_COOKIES
#pragma multi_compile_fragment _ _VOLUMETRICS_ENABLED
#pragma multi_compile_fog
#pragma skip_variants FOG_LINEAR FOG_EXP
//#pragma multi_compile_fragment _ DEBUG_DISPLAY
#pragma multi_compile_fragment _ _DETAILS_ON
//#pragma multi_compile_fragment _ _EMISSION_ON

#if defined(LITMAS_FEATURE_LIGHTMAPPING)
    #pragma multi_compile _ LIGHTMAP_ON
    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
    #pragma multi_compile _ DYNAMICLIGHTMAP_ON
    #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
    #pragma multi_compile _ SHADOWS_SHADOWMASK
#endif


#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SLZLighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SLZBlueNoise.hlsl"
#if !defined(SHADER_API_MOBILE) && defined(LITMAS_FEATURE_SSR)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SLZLightingSSR.hlsl"
#endif

#if defined(LITMAS_FEATURE_TP)
#include "Include/Triplanar.hlsl"
#endif

#if defined(LITMAS_FEATURE_IMPACTS)
#include "LitMASInclude/PosespaceImpacts.hlsl"
#endif

struct VertIn
{
    float4 vertex   : POSITION;
    float3 normal    : NORMAL;
    float4 tangent   : TANGENT;
    float4 uv0       : TEXCOORD0;
    float4 uv1      : TEXCOORD1;
    float4 uv2      : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertOut
{
    float4 vertex       : SV_POSITION;
    float4 uv0XY_bitZ_fog: TEXCOORD0;
#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    float4 uv1          : TEXCOORD1;
#endif
    half4 SHVertLights  : TEXCOORD2;
    half4 normXYZ_tanX  : TEXCOORD3;
#if defined(LITMAS_FEATURE_TS_NORMALS)
    half4 tanYZ_bitXY   : TEXCOORD4;
#endif
    float3 wPos         : TEXCOORD5;

#if defined(LITMAS_FEATURE_IMPACTS)
    float3 unskinnedObjPos : TEXCOORD6;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

TEXTURE2D(_BumpMap);
TEXTURE2D(_MetallicGlossMap);
TEXTURE2D(_EmissionMap);

TEXTURE2D(_DetailMap);
SAMPLER(sampler_DetailMap);

#if defined(LITMAS_FEATURE_WHITEBOARD)
    TEXTURE2D(_PenMap);
#endif


CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float4 _DetailMap_ST;
    half4 _BaseColor;
    half  _Details;
    half  _Normals;
    half  _Emission;
    half4 _EmissionColor;
    half  _EmissionFalloff;
    half  _BakedMutiplier;

#if defined(LITMAS_FEATURE_TP)
    half  _DetailsuseLocalUVs;
    half _RotateUVs;
    half _UVScaler;
#endif

#if defined(LITMAS_FEATURE_SSR)
    half  _SSRSmoothPow;
#endif

#if defined(LITMAS_FEATURE_WHITEBOARD)
    float4 _PenMap_ST;
    half  _PenMono;
    half4  _PenMonoColor;
#endif

#if defined(LITMAS_FEATURE_IMPACTS)
    float4x4 EllipsoidPosArray[HitArrayCount];
    int _NumberOfHits;
    float4 _HitColor;
#endif

CBUFFER_END

half3 OverlayBlendDetail(half source, half3 destination)
{
    half3 switch0 = round(destination); // if destination >= 0.5 then 1, else 0 assuming 0-1 input
    half3 blendGreater = mad(mad(2.0, destination, -2.0), 1.0 - source, 1.0); // (2.0 * destination - 2.0) * ( 1.0 - source) + 1.0
    half3 blendLesser = (2.0 * source) * destination;
    return mad(switch0, blendGreater, mad(-switch0, blendLesser, blendLesser)); // switch0 * blendGreater + (1 - switch0) * blendLesser 
    //return half3(destination.r > 0.5 ? blendGreater.r : blendLesser.r,
    //             destination.g > 0.5 ? blendGreater.g : blendLesser.g,
    //             destination.b > 0.5 ? blendGreater.b : blendLesser.b
    //            );
}


VertOut vert(VertIn v)
{
    VertOut o = (VertOut)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.wPos = TransformObjectToWorld(v.vertex.xyz);
    o.vertex = TransformWorldToHClip(o.wPos);
    o.uv0XY_bitZ_fog.xy = v.uv0.xy;

#if defined(LIGHTMAP_ON) || defined(DIRLIGHTMAP_COMBINED)
    OUTPUT_LIGHTMAP_UV(v.uv1.xy, unity_LightmapST, o.uv1.xy);
#endif

#ifdef DYNAMICLIGHTMAP_ON
    OUTPUT_LIGHTMAP_UV(v.uv2.xy, unity_DynamicLightmapST, o.uv1.zw);
#endif

    // Calculate tangent to world basis vectors
#if defined(LITMAS_FEATURE_TS_NORMALS)
    VertexNormalInputs ntb = GetVertexNormalInputs(v.normal, v.tangent);
    o.normXYZ_tanX = half4(ntb.normalWS, ntb.tangentWS.x);
    o.tanYZ_bitXY = half4(ntb.tangentWS.yz, ntb.bitangentWS.xy);
    o.uv0XY_bitZ_fog.z = ntb.bitangentWS.z;
#else
    o.normXYZ_tanX = half4(TransformObjectToWorldNormal(v.normal, false), 0);
    o.uv0XY_bitZ_fog.z = v.tangent.w; //Avoid optimization that would remove the tangent from the vertex input (causes issues) 
#endif

    // Exp2 fog
    half clipZ_0Far = UNITY_Z_0_FAR_FROM_CLIPSPACE(o.vertex.z);
    o.uv0XY_bitZ_fog.w = unity_FogParams.x * clipZ_0Far;

    o.SHVertLights = 0;
    // Calculate vertex lights and L2 probe lighting on quest 

    o.SHVertLights.xyz = VertexLighting(o.wPos, o.normXYZ_tanX.xyz);
#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON) && defined(SHADER_API_MOBILE)
    o.SHVertLights.xyz += SampleSHVertex(o.normXYZ_tanX.xyz);
#endif

#if defined(LITMAS_FEATURE_IMPACTS)
    o.unskinnedObjPos = v.uv2;
#endif

    return o;
}

half4 frag(VertOut i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Read Input Data---------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/


    #if defined(LITMAS_FEATURE_TP) /*-Triplanar------------------------------------------------------------------------------*/

        float2 uvTP;
        half3x3 TStoWsTP;
        half2 scale = 1.0/_UVScaler;
        
        #if defined(_EXPENSIVE_TP)
            tpDerivatives tpDD;
            GetDirectionalDerivatives(i.wPos, tpDD);
            half2 ddxTP, ddyTP;
            GetTPUVExpensive(uvTP, ddxTP, ddyTP, TStoWsTP, i.wPos, normalize(i.normXYZ_tanX.xyz), tpDD);
            ddxTP = _RotateUVs ? half2(-ddxTP.y, ddxTP.x) : ddxTP;
            ddyTP = _RotateUVs ? half2(-ddyTP.y, ddyTP.x) : ddyTP;
            half2 ddxMain = ddxTP * scale;
            half2 ddyMain = ddyTP * scale;
        #else
            GetTPUVCheap(uvTP, TStoWsTP, i.wPos, normalize(i.normXYZ_tanX.xyz));
        #endif
        
        uvTP = _RotateUVs ? float2(-uvTP.y, uvTP.x) : uvTP;
        float2 uv_main = mad(uvTP, scale, _BaseMap_ST.zw);
        half4 albedo = SLZ_SAMPLE_TP_MAIN(_BaseMap, sampler_BaseMap, uv_main) * _BaseColor;
        half3 mas = SLZ_SAMPLE_TP_MAIN(_MetallicGlossMap, sampler_BaseMap, uv_main).rgb;


    #else /*-Non-Triplanar---------------------------------------------------------------------------------------------------*/

        float2 uv_main = mad(float2(i.uv0XY_bitZ_fog.xy), _BaseMap_ST.xy, _BaseMap_ST.zw);
        float2 uv_detail = mad(float2(i.uv0XY_bitZ_fog.xy), _DetailMap_ST.xy, _DetailMap_ST.zw);
        half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv_main) * _BaseColor;
        half3 mas = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_BaseMap, uv_main).rgb;

    #endif

    #if defined(LITMAS_FEATURE_WHITEBOARD) /*-Whiteboard---------------------------------------------------------------------*/

        float2 uv_pen = mad(float2(i.uv0XY_bitZ_fog.xy), _PenMap_ST.xy, _PenMap_ST.zw);
        half4 penMap = SAMPLE_TEXTURE2D(_PenMap, sampler_BaseMap, uv_pen);
        penMap = _PenMono > 0.5 ? penMap.rrrr : penMap;
        penMap.rgb = _PenMono > 0.5 ? penMap.rgb * _PenMonoColor.rgb : penMap.rgb;
        albedo.rgb = penMap.rgb + albedo.rgb * (1.0h - penMap.a);

    #endif

    half metallic = mas.r;
    half ao = mas.g;
    half smoothness = mas.b;

#if defined(LITMAS_FEATURE_IMPACTS)
    half2 impactUV = GetClosestImpactUV(i.unskinnedObjPos, EllipsoidPosArray, HitArrayCount);
    half4 impactMASI = SampleHitTexture(impactUV);
    impactMASI.a = impactUV.x > 0.999 ? 0 : impactMASI.a;
    albedo = lerp(_HitColor, albedo, impactMASI.a);
    metallic = lerp(impactMASI.r, metallic, impactMASI.a);
    smoothness = lerp(impactMASI.b, smoothness, impactMASI.a);
    ao = min(ao, max(impactMASI.a, impactMASI.g));
#endif

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Sample Normal Map-------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    half3 normalTS = half3(0, 0, 1);
    half  geoSmooth = 1;
    half4 normalMap = half4(0, 0, 1, 0);

    #if defined(LITMAS_FEATURE_TS_NORMALS) /*-Tangent Space Normals----------------------------------------------------------*/

        normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BaseMap, uv_main);
        normalTS = UnpackNormal(normalMap);
        normalTS = _Normals ? normalTS : half3(0, 0, 1);
        geoSmooth = _Normals ? normalMap.b : 1.0;
        smoothness = saturate(smoothness + geoSmooth - 1.0);

    #elif defined(LITMAS_FEATURE_TP) /*-Triplanar Psuedo tangent space normals-----------------------------------------------*/

        normalMap = SLZ_SAMPLE_TP_MAIN(_BumpMap, sampler_BaseMap, uv_main);
        normalTS = UnpackNormal(normalMap);
        normalTS = _Normals ? normalTS : half3(0, 0, 1);
        normalTS = _RotateUVs ? half3(normalTS.y, -normalTS.x, normalTS.z) : normalTS;
        geoSmooth = _Normals ? normalMap.b : 1.0;
        smoothness = saturate(smoothness + geoSmooth - 1.0);

    #endif

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Read Detail Map---------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    #if defined(_DETAILS_ON) 

        #if defined(LITMAS_FEATURE_TP) /*-Triplanar--------------------------------------------------------------------------*/

            float2 uv_detail = mad(uvTP, _DetailMap_ST.xx, _DetailMap_ST.zw);
            uv_detail = _DetailsuseLocalUVs ? mad(float2(i.uv0XY_bitZ_fog.xy), _DetailMap_ST.xy, _DetailMap_ST.zw) : uv_detail;
            #if defined(_EXPENSIVE_TP)
                half2 ddxDetail = ddx(uv_detail);
                half2 ddyDetail = ddy(uv_detail);
                ddxDetail = _DetailsuseLocalUVs ? ddxDetail : ddxTP * _DetailMap_ST.xx;
                ddyDetail = _DetailsuseLocalUVs ? ddyDetail : ddyTP * _DetailMap_ST.xx;
            #endif
            half4 detailMap = SLZ_SAMPLE_TP_DETAIL(_DetailMap, sampler_DetailMap, uv_detail);
            half3 detailTS = half3(2.0 * detailMap.ag - 1.0, 1.0);
            detailTS = _RotateUVs && !(_DetailsuseLocalUVs) ? half3(detailTS.y, -detailTS.x, detailTS.z) : detailTS;
            normalTS = BlendNormal(normalTS, detailTS);

        #elif defined(LITMAS_FEATURE_TS_NORMALS) /*-Tangent Space Normals----------------------------------------------------*/

            half4 detailMap = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, uv_detail);
            half3 detailTS = half3(2.0 * detailMap.ag - 1.0, 1.0);
            normalTS = BlendNormal(normalTS, detailTS);

        #else /*-Default-----------------------------------------------------------------------------------------------------*/

            half4 detailMap = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, uv_detail);

        #endif
       
        smoothness = saturate(2.0 * detailMap.b * smoothness);
        albedo.rgb = OverlayBlendDetail(detailMap.r, albedo.rgb);

    #endif

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Transform Normals To Worldspace-----------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    

    #if defined(LITMAS_FEATURE_TS_NORMALS) /*-Tangent Space Normals----------------------------------------------------------*/
        half3 normalWS = i.normXYZ_tanX.xyz;
        half3x3 TStoWS = half3x3(
            i.normXYZ_tanX.w, i.tanYZ_bitXY.z,    normalWS.x,
            i.tanYZ_bitXY.x,  i.tanYZ_bitXY.w,    normalWS.y,
            i.tanYZ_bitXY.y,  i.uv0XY_bitZ_fog.z, normalWS.z
            );

        normalWS = mul(TStoWS, normalTS);
        normalWS = normalize(normalWS);

    #elif defined(LITMAS_FEATURE_TP) /*-Triplanar----------------------------------------------------------------------------*/

        half3 normalWS = mul(TStoWsTP, normalTS);
        normalWS = normalize(normalWS);

    #else /*-Default---------------------------------------------------------------------------------------------------------*/

        half3 normalWS = i.normXYZ_tanX.xyz;

    #endif

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Lighting Calculations---------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    #if !defined(SHADER_API_MOBILE) && !defined(LITMAS_FEATURE_TP) // Specular antialiasing based on normal derivatives. Only on PC to avoid cost of derivatives on Quest
        smoothness = min(smoothness, SLZGeometricSpecularAA(normalWS));
    #endif


    #if defined(LIGHTMAP_ON)
        SLZFragData fragData = SLZGetFragData(i.vertex, i.wPos, normalWS, i.uv1.xy, i.uv1.zw, i.SHVertLights.xyz);
    #else
        SLZFragData fragData = SLZGetFragData(i.vertex, i.wPos, normalWS, float2(0, 0), float2(0, 0), i.SHVertLights.xyz);
    #endif

    half4 emission = half4(0,0,0,0);

    #if defined(LITMAS_FEATURE_EMISSION) /*-Emission-------------------------------------------------------------------------*/

        UNITY_BRANCH if (_Emission)
        {
            emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, uv_main) * _EmissionColor;
            emission.rgb *= lerp(albedo.rgb, half3(1, 1, 1), emission.a);
            emission.rgb *= pow(abs(fragData.NoV), _EmissionFalloff);
        }

    #endif

    #if defined(LITMAS_FEATURE_SSR) && defined(_SSR_ON) && !defined(SHADER_API_MOBILE) /*-SSR--------------------------------*/

        smoothness = pow(smoothness, _SSRSmoothPow);

    #endif

    SLZSurfData surfData = SLZGetSurfDataMetallicGloss(albedo.rgb, saturate(metallic), saturate(smoothness), ao, emission.rgb);
    half4 color = half4(1, 1, 1, 1);

    #if defined(LITMAS_FEATURE_SSR) && defined(_SSR_ON) && !defined(SHADER_API_MOBILE) /*-SSR--------------------------------*/

        half4 noiseRGBA = GetScreenNoiseRGBA(fragData.screenUV);
        color.rgb = SLZPBRFragmentSSR(fragData, surfData, i.normXYZ_tanX.xyz, noiseRGBA);

    #else /*-Default---------------------------------------------------------------------------------------------------------*/

        color.rgb = SLZPBRFragment(fragData, surfData);

    #endif

    color.rgb = MixFog(color.rgb, -fragData.viewDir, i.uv0XY_bitZ_fog.w);
    color = Volumetrics(color, fragData.position);

    return color;
}