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


#endif

#pragma multi_compile_fragment _ _LIGHT_COOKIES
#pragma multi_compile _ SHADOWS_SHADOWMASK
#pragma multi_compile_fragment _ _VOLUMETRICS_ENABLED
#pragma multi_compile_fog
#pragma skip_variants FOG_LINEAR FOG_EXP
//#pragma multi_compile_fragment _ DEBUG_DISPLAY
#pragma multi_compile_fragment _ _DETAILS_ON
//#pragma multi_compile_fragment _ _EMISSION_ON


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

// Begin Injection INCLUDES from Injection_AudioLink.hlsl ----------------------------------------------------------
#include "AudioLink/Shaders/AudioLink.cginc"
// End Injection INCLUDES from Injection_AudioLink.hlsl ----------------------------------------------------------



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

// Begin Injection INTERPOLATORS from Injection_NormalMaps.hlsl ----------------------------------------------------------
    half4 tanYZ_bitXY : TEXCOORD5;
// End Injection INTERPOLATORS from Injection_NormalMaps.hlsl ----------------------------------------------------------

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
// Begin Injection UNIFORMS from Injection_AudioLink.hlsl ----------------------------------------------------------
TEXTURE2D(_AudioLinkMap);
SAMPLER(sampler_AudioLinkMap);
TEXTURE2D(_AudioLinkNoise);
SAMPLER(sampler_AudioLinkNoise);
// End Injection UNIFORMS from Injection_AudioLink.hlsl ----------------------------------------------------------

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
// Begin Injection MATERIAL_CBUFFER from Injection_NormalMap_CBuffer.hlsl ----------------------------------------------------------
float4 _DetailMap_ST;
half  _Details;
half  _Normals;
// End Injection MATERIAL_CBUFFER from Injection_NormalMap_CBuffer.hlsl ----------------------------------------------------------
// Begin Injection MATERIAL_CBUFFER from Injection_AudioLink.hlsl ----------------------------------------------------------
    half  _AudioInputBoost;
    half  _SmoothstepBlend;
    half  _AudioLinkBaseBlend;
    half4 _LowsColor;
    half4 _MidsColor;
    half4 _HighsColor;
// End Injection MATERIAL_CBUFFER from Injection_AudioLink.hlsl ----------------------------------------------------------
// Begin Injection MATERIAL_CBUFFER from Injection_Emission.hlsl ----------------------------------------------------------
    half  _Emission;
    half4 _EmissionColor;
    half  _EmissionFalloff;
    half  _BakedMutiplier;
// End Injection MATERIAL_CBUFFER from Injection_Emission.hlsl ----------------------------------------------------------
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

// Begin Injection FUNCTIONS from Injection_AudioLink.hlsl ----------------------------------------------------------
half GetALChannelIntensity(half channelValue, half channelMask)
{
    half smoothstepMin = saturate(1.0 - (channelValue + _AudioInputBoost));
    half smoothstepMax = smoothstepMin + _SmoothstepBlend + 0.01;
    half output = smoothstep(smoothstepMin, smoothstepMax, channelMask);
    return output;
}

float GetALChronotensity()
{
    return (AudioLinkDecodeDataAsUInt(ALPASS_CHRONOTENSITY + uint2(0, 0)) % 628319) / 100000.0;
}

float ALPanU(float u, float velocity, float time)
{
    return u + frac(velocity*time);
}

// What even is this bullshit? I'm just copying this verbatim from the amplify version, this math makes no sense
half4 GetALNoise(float u, half3 viewDir, half2x3 tan2wrldXY) 
{
    float chrono = 0.5 * GetALChronotensity();
    chrono = (0.5 * _Time[1]) + chrono;
    u = ALPanU(u, 0.17, chrono);
    u = sin(6.0 * u);
    float2 uv1 = viewDir.xy;
    float2 uv2 = mul(tan2wrldXY, viewDir);
    float2 uv = lerp(uv1, uv2, u);
    return SAMPLE_TEXTURE2D(_AudioLinkNoise, sampler_AudioLinkNoise, uv);
}

half4 GetALChannelValue(half2 audioLinkMask, half channelValue, half4 channelColor, half4 noiseColor)
{
    half4 a = (channelColor * noiseColor);
    half aNoise = (audioLinkMask.x * _AudioLinkBaseBlend);
    half aMain = (audioLinkMask.y * channelValue);
    return a * (aNoise + aMain);
}

// End Injection FUNCTIONS from Injection_AudioLink.hlsl ----------------------------------------------------------

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

// Begin Injection VERTEX_NORMALS from Injection_NormalMaps.hlsl ----------------------------------------------------------
	VertexNormalInputs ntb = GetVertexNormalInputs(v.normal, v.tangent);
	o.normXYZ_tanX = half4(ntb.normalWS, ntb.tangentWS.x);
	o.tanYZ_bitXY = half4(ntb.tangentWS.yz, ntb.bitangentWS.xy);
	o.uv0XY_bitZ_fog.z = ntb.bitangentWS.z;
// End Injection VERTEX_NORMALS from Injection_NormalMaps.hlsl ----------------------------------------------------------

    o.SHVertLights = 0;
    // Calculate vertex lights and L2 probe lighting on quest 
    o.SHVertLights.xyz = VertexLighting(o.wPos, o.normXYZ_tanX.xyz);
#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON) && defined(SHADER_API_MOBILE)
    o.SHVertLights.xyz += SampleSHVertex(o.normXYZ_tanX.xyz);
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

    float2 uv_main = mad(float2(i.uv0XY_bitZ_fog.xy), _BaseMap_ST.xy, _BaseMap_ST.zw);
    float2 uv_detail = mad(float2(i.uv0XY_bitZ_fog.xy), _DetailMap_ST.xy, _DetailMap_ST.zw);
    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv_main) * _BaseColor;
    half4 mas = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_BaseMap, uv_main);


    half metallic = mas.r;
    half ao = mas.g;
    half smoothness = mas.b;


/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Sample Normal Map-------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    half3 normalTS = half3(0, 0, 1);
    half  geoSmooth = 1;
    half4 normalMap = half4(0, 0, 1, 0);

// Begin Injection NORMAL_MAP from Injection_NormalMaps.hlsl ----------------------------------------------------------
	normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BaseMap, uv_main);
	normalTS = UnpackNormal(normalMap);
	normalTS = _Normals ? normalTS : half3(0, 0, 1);
	geoSmooth = _Normals ? normalMap.b : 1.0;
	smoothness = saturate(smoothness + geoSmooth - 1.0);
// End Injection NORMAL_MAP from Injection_NormalMaps.hlsl ----------------------------------------------------------

/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Read Detail Map---------------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

    #if defined(_DETAILS_ON) 

// Begin Injection DETAIL_MAP from Injection_NormalMaps.hlsl ----------------------------------------------------------
		half4 detailMap = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, uv_detail);
		half3 detailTS = half3(2.0 * detailMap.ag - 1.0, 1.0);
		normalTS = BlendNormal(normalTS, detailTS);
// End Injection DETAIL_MAP from Injection_NormalMaps.hlsl ----------------------------------------------------------
       
        smoothness = saturate(2.0 * detailMap.b * smoothness);
        albedo.rgb = OverlayBlendDetail(detailMap.r, albedo.rgb);

    #endif


/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Transform Normals To Worldspace-----------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/

// Begin Injection NORMAL_TRANSFORM from Injection_NormalMaps.hlsl ----------------------------------------------------------
	half3 normalWS = i.normXYZ_tanX.xyz;
	half3x3 TStoWS = half3x3(
		i.normXYZ_tanX.w, i.tanYZ_bitXY.z, normalWS.x,
		i.tanYZ_bitXY.x, i.tanYZ_bitXY.w, normalWS.y,
		i.tanYZ_bitXY.y, i.uv0XY_bitZ_fog.z, normalWS.z
		);
	normalWS = mul(TStoWS, normalTS);
	normalWS = normalize(normalWS);
// End Injection NORMAL_TRANSFORM from Injection_NormalMaps.hlsl ----------------------------------------------------------


/*---------------------------------------------------------------------------------------------------------------------------*/
/*---Lighting Calculations---------------------------------------------------------------------------------------------------*/
/*---------------------------------------------------------------------------------------------------------------------------*/
    
// Begin Injection SPEC_AA from Injection_NormalMaps.hlsl ----------------------------------------------------------
	#if !defined(SHADER_API_MOBILE) && !defined(LITMAS_FEATURE_TP) // Specular antialiasing based on normal derivatives. Only on PC to avoid cost of derivatives on Quest
		smoothness = min(smoothness, SLZGeometricSpecularAA(normalWS));
	#endif
// End Injection SPEC_AA from Injection_NormalMaps.hlsl ----------------------------------------------------------


    #if defined(LIGHTMAP_ON)
        SLZFragData fragData = SLZGetFragData(i.vertex, i.wPos, normalWS, i.uv1.xy, i.uv1.zw, i.SHVertLights.xyz);
    #else
        SLZFragData fragData = SLZGetFragData(i.vertex, i.wPos, normalWS, float2(0, 0), float2(0, 0), i.SHVertLights.xyz);
    #endif

    half4 emission = half4(0,0,0,0);

// Begin Injection EMISSION from Injection_AudioLink.hlsl ----------------------------------------------------------
    #if defined(PASS_META)
    half4 audioLinkNoise = GetALNoise(uv_main.x, half3(1, 1, 1), (half2x3)TStoWS);
    #else
    half4 audioLinkNoise = GetALNoise(uv_main.x, fragData.viewDir, (half2x3)TStoWS);
    #endif
    half4 audioLinkMask = SAMPLE_TEXTURE2D(_AudioLinkMap, sampler_AudioLinkMap, uv_main);
    half audioLows = AudioLinkData(ALPASS_AUDIOBASS).x;
    audioLows = GetALChannelIntensity(audioLows, audioLinkMask.r);
    emission += GetALChannelValue(audioLinkMask.ra, audioLows, _LowsColor, audioLinkNoise);
  
    half audioMids = AudioLinkData(ALPASS_AUDIOLOWMIDS).x;
    audioMids = GetALChannelIntensity(audioMids, audioLinkMask.g);
    emission += GetALChannelValue(audioLinkMask.ga, audioMids, _MidsColor, audioLinkNoise);

    half audioHighs = AudioLinkData(ALPASS_AUDIOHIGHMIDS).x + AudioLinkData(ALPASS_AUDIOTREBLE).x * 0.5;
    audioHighs = GetALChannelIntensity(audioHighs, audioLinkMask.b);
    emission += GetALChannelValue(audioLinkMask.ba, audioHighs, _HighsColor, audioLinkNoise);

// End Injection EMISSION from Injection_AudioLink.hlsl ----------------------------------------------------------
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


        color.rgb = SLZPBRFragment(fragData, surfData);


    color.rgb = MixFog(color.rgb, -fragData.viewDir, i.uv0XY_bitZ_fog.w);
    color = Volumetrics(color, fragData.position);
    return color;
}