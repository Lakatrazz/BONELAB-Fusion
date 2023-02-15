
#define SHADERPASS SHADERPASS_FORWARD
#define _NORMAL_DROPOFF_TS 1
#define _EMISSION
#define _NORMALMAP 1

#if defined(SHADER_API_MOBILE)
    #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX
    //#!INJECT_POINT MOBILE_DEFINES
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

    //#!INJECT_POINT STANDALONE_DEFINES

#endif

#pragma multi_compile_fragment _ _LIGHT_COOKIES
#pragma multi_compile _ SHADOWS_SHADOWMASK
#pragma multi_compile_fragment _ _VOLUMETRICS_ENABLED
#pragma multi_compile_fog
#pragma skip_variants FOG_LINEAR FOG_EXP
//#pragma multi_compile_fragment _ DEBUG_DISPLAY
#pragma multi_compile_fragment _ _DETAILS_ON
//#pragma multi_compile_fragment _ _EMISSION_ON

//#!INJECT_POINT UNIVERSAL_DEFINES

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

//#!INJECT_POINT INCLUDES



struct VertIn
{
    float4 vertex   : POSITION;
    float3 normal    : NORMAL;
    float4 tangent   : TANGENT;
    //#!TEXCOORD float4 uv0 0
    //#!TEXCOORD float4 uv1 0
    //#!TEXCOORD float4 uv2 0
    //#!INJECT_POINT VERTEX_IN
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertOut
{
    float4 vertex       : SV_POSITION;
    //#!TEXCOORD float4 uv0XY_bitZ_fog 1
#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    //#!TEXCOORD float4 uv1 1
#endif
    //#!TEXCOORD half4 SHVertLights 1
    //#!TEXCOORD half4 normXYZ_tanX 1
    //#!TEXCOORD float3 wPos 1

    //#!INJECT_POINT INTERPOLATORS

    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

TEXTURE2D(_BumpMap);
TEXTURE2D(_MetallicGlossMap);

TEXTURE2D(_DetailMap);
SAMPLER(sampler_DetailMap);

//#!INJECT_POINT UNIFORMS

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    //#!INJECT_POINT MATERIAL_CBUFFER
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

//#!INJECT_POINT FUNCTIONS

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

    //#!INJECT_POINT VERTEX_NORMALS
    //#!INJECT_DEFAULT
    o.normXYZ_tanX.xyz = v.normal;
    o.normXYZ_tanX.w = 0h;
    o.uv0XY_bitZ_fog.z = v.tangent.w; // avoid having the tangent optimized out to prevent issues with the depth-prepass
    //#!INJECT_END

    o.SHVertLights = 0;
    // Calculate vertex lights and L2 probe lighting on quest 
    o.SHVertLights.xyz = VertexLighting(o.wPos, o.normXYZ_tanX.xyz);
#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON) && defined(SHADER_API_MOBILE)
    o.SHVertLights.xyz += SampleSHVertex(o.normXYZ_tanX.xyz);
#endif

    //#!INJECT_POINT VERTEX_END
    return o;
}

half4 frag(VertOut i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Read Input Data---------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    //#!INJECT_POINT FRAG_READ_INPUTS
    //#!INJECT_DEFAULT
    float2 uv_main = mad(float2(i.uv0XY_bitZ_fog.xy), _BaseMap_ST.xy, _BaseMap_ST.zw);
    float2 uv_detail = mad(float2(i.uv0XY_bitZ_fog.xy), _DetailMap_ST.xy, _DetailMap_ST.zw);
    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv_main);
    half4 mas = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_BaseMap, uv_main);
    //#!INJECT_END


    //#!INJECT_POINT FRAG_POST_READ

    albedo *= _BaseColor;
    half metallic = mas.r;
    half ao = mas.g;
    half smoothness = mas.b;

    //#!INJECT_POINT FRAG_POST_INPUTS

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Sample Normal Map-------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    half3 normalTS = half3(0, 0, 1);
    half  geoSmooth = 1;
    half4 normalMap = half4(0, 0, 1, 0);

    //#!INJECT_POINT NORMAL_MAP

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Read Detail Map---------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    #if defined(_DETAILS_ON) 

        //#!INJECT_POINT DETAIL_MAP
       
        smoothness = saturate(2.0 * detailMap.b * smoothness);
        albedo.rgb = OverlayBlendDetail(detailMap.r, albedo.rgb);

    #endif

    //#!INJECT_POINT PRE_NORMAL_TS_TO_WS

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Transform Normals To Worldspace-----------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    //#!INJECT_POINT NORMAL_TRANSFORM    


/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Lighting Calculations---------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/
    
    //#!INJECT_POINT SPEC_AA
    //#!INJECT_DEFAULT
    #if !defined(SHADER_API_MOBILE) && !defined(LITMAS_FEATURE_TP) // Specular antialiasing based on normal derivatives. Only on PC to avoid cost of derivatives on Quest
        smoothness = min(smoothness, SLZGeometricSpecularAA(i.normXYZ_tanX.xyz));
    #endif
    //#!INJECT_END

    //#!INJECT_POINT PRE_FRAGDATA

    #if defined(LIGHTMAP_ON)
        SLZFragData fragData = SLZGetFragData(i.vertex, i.wPos, normalWS, i.uv1.xy, i.uv1.zw, i.SHVertLights.xyz);
    #else
        SLZFragData fragData = SLZGetFragData(i.vertex, i.wPos, normalWS, float2(0, 0), float2(0, 0), i.SHVertLights.xyz);
    #endif

    half4 emission = half4(0,0,0,0);

    //#!INJECT_POINT EMISSION

    //#!INJECT_POINT PRE_SURFDATA

    SLZSurfData surfData = SLZGetSurfDataMetallicGloss(albedo.rgb, saturate(metallic), saturate(smoothness), ao, emission.rgb);
    half4 color = half4(1, 1, 1, 1);

    //#!INJECT_POINT PRE_LIGHTING_CALC

    //#!INJECT_POINT LIGHTING_CALC
    //#!INJECT_DEFAULT
        color.rgb = SLZPBRFragment(fragData, surfData);
    //#!INJECT_END


    //#!INJECT_POINT VOLUMETRIC_FOG
    //#!INJECT_DEFAULT
    color.rgb = MixFog(color.rgb, -fragData.viewDir, i.uv0XY_bitZ_fog.w);
    color = Volumetrics(color, fragData.position);
    //#!INJECT_END
    return color;
}