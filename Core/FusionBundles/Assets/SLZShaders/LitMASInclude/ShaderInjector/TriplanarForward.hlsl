/*-----------------------------------------------------------------------------------------------------*
 *-----------------------------------------------------------------------------------------------------*
 * WARNING: THIS FILE WAS CREATED WITH SHADERINJECTOR, AND SHOULD NOT BE EDITED DIRECTLY. MODIFY THE   *
 * BASE INCLUDE AND INJECTED FILES INSTEAD, AND REGENERATE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!   *
 *-----------------------------------------------------------------------------------------------------*
 *-----------------------------------------------------------------------------------------------------*/


#define SHADERPASS SHADERPASS_FORWARD
#define _NORMAL_DROPOFF_TS 1
#define _EMISSION
#define _NORMALMAP 1

#if defined(SHADER_API_MOBILE)
    #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX
#else              
    #pragma multi_compile           _   _MAIN_LIGHT_SHADOWS_CASCADE
    #pragma multi_compile_fragment  _   _SCREEN_SPACE_OCCLUSION
    
    #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
    #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
    #pragma multi_compile_fragment _ _SHADOWS_SOFT
    
    #pragma multi_compile_fragment _REFLECTION_PROBE_BLENDING
    //#pragma shader_feature_fragment _REFLECTION_PROBE_BOX_PROJECTION
    // We don't need a keyword for this! the w component of the probe position already branches box vs non-box, & so little cost on pc it doesn't matter
    #define _REFLECTION_PROBE_BOX_PROJECTION 

// Begin Injection STANDALONE_DEFINES from Injection_SSR.hlsl ----------------------------------------------------------
#pragma multi_compile _ _SLZ_SSR_ENABLED
#pragma shader_feature_local _ _NO_SSR
#if defined(_SLZ_SSR_ENABLED) && !defined(_NO_SSR) && !defined(SHADER_API_MOBILE)
    #define _SSR_ENABLED
#endif
// End Injection STANDALONE_DEFINES from Injection_SSR.hlsl ----------------------------------------------------------

#endif

#pragma multi_compile_fragment _ _LIGHT_COOKIES
#pragma multi_compile _ SHADOWS_SHADOWMASK
#pragma multi_compile_fragment _ _VOLUMETRICS_ENABLED
#pragma multi_compile_fog
#pragma skip_variants FOG_LINEAR FOG_EXP
//#pragma multi_compile_fragment _ DEBUG_DISPLAY
#pragma multi_compile_fragment _ _DETAILS_ON
//#pragma multi_compile_fragment _ _EMISSION_ON

// Begin Injection UNIVERSAL_DEFINES from Injection_Triplanar.hlsl ----------------------------------------------------------
    #pragma multi_compile_local_fragment _ _EXPENSIVE_TP

    #if defined(_EXPENSIVE_TP)
        #define SLZ_SAMPLE_TP_MAIN(tex, sampl, uv) SAMPLE_TEXTURE2D_GRAD(tex, sampl, uv, ddxMain, ddyMain)
        #define SLZ_SAMPLE_TP_DETAIL(tex, sampl, uv) SAMPLE_TEXTURE2D_GRAD(tex, sampl, uv, ddxDetail, ddyDetail)
    #else
        #define SLZ_SAMPLE_TP_MAIN(tex, sampl, uv) SAMPLE_TEXTURE2D(tex, sampl, uv)
        #define SLZ_SAMPLE_TP_DETAIL(tex, sampl, uv) SAMPLE_TEXTURE2D(tex, sampl, uv)
    #endif
// End Injection UNIVERSAL_DEFINES from Injection_Triplanar.hlsl ----------------------------------------------------------

#if defined(LITMAS_FEATURE_LIGHTMAPPING)
    #pragma multi_compile _ LIGHTMAP_ON
    #pragma multi_compile _ DYNAMICLIGHTMAP_ON
    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
    #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
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

// Begin Injection INCLUDES from Injection_Triplanar.hlsl ----------------------------------------------------------
#include "Include/Triplanar.hlsl"
// End Injection INCLUDES from Injection_Triplanar.hlsl ----------------------------------------------------------
// Begin Injection INCLUDES from Injection_SSR.hlsl ----------------------------------------------------------
#if !defined(SHADER_API_MOBILE)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SLZLightingSSR.hlsl"
#endif
// End Injection INCLUDES from Injection_SSR.hlsl ----------------------------------------------------------



struct VertIn
{
    float4 vertex   : POSITION;
    float3 normal    : NORMAL;
    float4 tangent   : TANGENT;
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv2 : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertOut
{
    float4 vertex       : SV_POSITION;
    float4 uv0XY_bitZ_fog : TEXCOORD0;
#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    float4 uv1 : TEXCOORD1;
#endif
    half4 SHVertLights : TEXCOORD2;
    half4 normXYZ_tanX : TEXCOORD3;
    float3 wPos : TEXCOORD4;

// Begin Injection INTERPOLATORS from Injection_SSR.hlsl ----------------------------------------------------------
    float4 lastVertex : TEXCOORD5;
// End Injection INTERPOLATORS from Injection_SSR.hlsl ----------------------------------------------------------

    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

TEXTURE2D(_BumpMap);
TEXTURE2D(_MetallicGlossMap);

TEXTURE2D(_DetailMap);
SAMPLER(sampler_DetailMap);

// Begin Injection UNIFORMS from Injection_Emission.hlsl ----------------------------------------------------------
TEXTURE2D(_EmissionMap);
// End Injection UNIFORMS from Injection_Emission.hlsl ----------------------------------------------------------

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
// Begin Injection MATERIAL_CBUFFER from Injection_Triplanar.hlsl ----------------------------------------------------------
    float4 _DetailMap_ST;
    half  _Details;
    half  _Normals;
    half  _DetailsuseLocalUVs;
    half _RotateUVs;
    half _UVScaler;
// End Injection MATERIAL_CBUFFER from Injection_Triplanar.hlsl ----------------------------------------------------------
// Begin Injection MATERIAL_CBUFFER from Injection_Emission.hlsl ----------------------------------------------------------
    half  _Emission;
    half4 _EmissionColor;
    half  _EmissionFalloff;
    half  _BakedMutiplier;
// End Injection MATERIAL_CBUFFER from Injection_Emission.hlsl ----------------------------------------------------------
// Begin Injection MATERIAL_CBUFFER from Injection_SSR_CBuffer.hlsl ----------------------------------------------------------
    float _SSRTemporalMul;
// End Injection MATERIAL_CBUFFER from Injection_SSR_CBuffer.hlsl ----------------------------------------------------------
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

    // Exp2 fog
    half clipZ_0Far = UNITY_Z_0_FAR_FROM_CLIPSPACE(o.vertex.z);
    o.uv0XY_bitZ_fog.w = unity_FogParams.x * clipZ_0Far;

// Begin Injection VERTEX_NORMALS from Injection_Triplanar.hlsl ----------------------------------------------------------
    o.normXYZ_tanX = half4(TransformObjectToWorldNormal(v.normal, false), 0);
    o.uv0XY_bitZ_fog.z = v.tangent.w; //Avoid optimization that would remove the tangent from the vertex input (causes issues)
// End Injection VERTEX_NORMALS from Injection_Triplanar.hlsl ----------------------------------------------------------

    o.SHVertLights = 0;
    // Calculate vertex lights and L2 probe lighting on quest 
    o.SHVertLights.xyz = VertexLighting(o.wPos, o.normXYZ_tanX.xyz);
#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON) && defined(SHADER_API_MOBILE)
    o.SHVertLights.xyz += SampleSHVertex(o.normXYZ_tanX.xyz);
#endif

// Begin Injection VERTEX_END from Injection_SSR.hlsl ----------------------------------------------------------
    #if defined(_SSR_ENABLED)
        float4 lastWPos = mul(GetPrevObjectToWorldMatrix(), v.vertex);
        o.lastVertex = mul(prevVP, lastWPos);
    #endif
// End Injection VERTEX_END from Injection_SSR.hlsl ----------------------------------------------------------
    return o;
}

half4 frag(VertOut i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Read Input Data---------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

// Begin Injection FRAG_READ_INPUTS from Injection_Triplanar.hlsl ----------------------------------------------------------

        /*-Triplanar---------------------------------------------------------------------------------------------------------*/

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

// End Injection FRAG_READ_INPUTS from Injection_Triplanar.hlsl ----------------------------------------------------------


    half metallic = mas.r;
    half ao = mas.g;
    half smoothness = mas.b;


/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Sample Normal Map-------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    half3 normalTS = half3(0, 0, 1);
    half  geoSmooth = 1;
    half4 normalMap = half4(0, 0, 1, 0);

// Begin Injection NORMAL_MAP from Injection_Triplanar.hlsl ----------------------------------------------------------

        /*-Triplanar Psuedo tangent space normals----------------------------------------------------------------------------*/
        normalMap = SLZ_SAMPLE_TP_MAIN(_BumpMap, sampler_BaseMap, uv_main);
        normalTS = UnpackNormal(normalMap);
        normalTS = _Normals ? normalTS : half3(0, 0, 1);
        normalTS = _RotateUVs ? half3(normalTS.y, -normalTS.x, normalTS.z) : normalTS;
        geoSmooth = _Normals ? normalMap.b : 1.0;
        smoothness = saturate(smoothness + geoSmooth - 1.0);

// End Injection NORMAL_MAP from Injection_Triplanar.hlsl ----------------------------------------------------------

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Read Detail Map---------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    #if defined(_DETAILS_ON) 

// Begin Injection DETAIL_MAP from Injection_Triplanar.hlsl ----------------------------------------------------------
        /*-Triplanar---------------------------------------------------------------------------------------------------------*/
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
// End Injection DETAIL_MAP from Injection_Triplanar.hlsl ----------------------------------------------------------
       
        smoothness = saturate(2.0 * detailMap.b * smoothness);
        albedo.rgb = OverlayBlendDetail(detailMap.r, albedo.rgb);

    #endif


/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Transform Normals To Worldspace-----------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

// Begin Injection NORMAL_TRANSFORM from Injection_Triplanar.hlsl ----------------------------------------------------------

    /*-Triplanar-------------------------------------------------------------------------------------------------------------*/
    half3 normalWS = mul(TStoWsTP, normalTS);
    normalWS = normalize(normalWS);

// End Injection NORMAL_TRANSFORM from Injection_Triplanar.hlsl ----------------------------------------------------------


/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Lighting Calculations---------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/
    
    #if !defined(SHADER_API_MOBILE) && !defined(LITMAS_FEATURE_TP) // Specular antialiasing based on normal derivatives. Only on PC to avoid cost of derivatives on Quest
        smoothness = min(smoothness, SLZGeometricSpecularAA(i.normXYZ_tanX.xyz));
    #endif


    #if defined(LIGHTMAP_ON)
        SLZFragData fragData = SLZGetFragData(i.vertex, i.wPos, normalWS, i.uv1.xy, i.uv1.zw, i.SHVertLights.xyz);
    #else
        SLZFragData fragData = SLZGetFragData(i.vertex, i.wPos, normalWS, float2(0, 0), float2(0, 0), i.SHVertLights.xyz);
    #endif

    half4 emission = half4(0,0,0,0);

// Begin Injection EMISSION from Injection_Emission.hlsl ----------------------------------------------------------
    UNITY_BRANCH if (_Emission)
    {
        emission += SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, uv_main) * _EmissionColor;
        emission.rgb *= lerp(albedo.rgb, half3(1, 1, 1), emission.a);
        emission.rgb *= pow(abs(fragData.NoV), _EmissionFalloff);
    }
// End Injection EMISSION from Injection_Emission.hlsl ----------------------------------------------------------


    SLZSurfData surfData = SLZGetSurfDataMetallicGloss(albedo.rgb, saturate(metallic), saturate(smoothness), ao, emission.rgb);
    half4 color = half4(1, 1, 1, 1);


// Begin Injection LIGHTING_CALC from Injection_SSR.hlsl ----------------------------------------------------------
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
// End Injection LIGHTING_CALC from Injection_SSR.hlsl ----------------------------------------------------------


// Begin Injection VOLUMETRIC_FOG from Injection_SSR.hlsl ----------------------------------------------------------
    #if !defined(_SSR_ENABLED)
        color.rgb = MixFog(color.rgb, -fragData.viewDir, i.uv0XY_bitZ_fog.w);
        color = Volumetrics(color, fragData.position);
    #endif
// End Injection VOLUMETRIC_FOG from Injection_SSR.hlsl ----------------------------------------------------------
    return color;
}