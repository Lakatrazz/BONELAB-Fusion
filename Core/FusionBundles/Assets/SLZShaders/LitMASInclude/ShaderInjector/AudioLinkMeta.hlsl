/*-----------------------------------------------------------------------------------------------------*
 *-----------------------------------------------------------------------------------------------------*
 * WARNING: THIS FILE WAS CREATED WITH SHADERINJECTOR, AND SHOULD NOT BE EDITED DIRECTLY. MODIFY THE   *
 * BASE INCLUDE AND INJECTED FILES INSTEAD, AND REGENERATE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!   *
 *-----------------------------------------------------------------------------------------------------*
 *-----------------------------------------------------------------------------------------------------*/

#define SHADERPASS SHADERPASS_META
#define PASS_META

#if defined(SHADER_API_MOBILE)


#else


#endif

#pragma shader_feature _ EDITOR_VISUALIZATION


#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

// Begin Injection INCLUDES from Injection_AudioLink.hlsl ----------------------------------------------------------
#include "AudioLink/Shaders/AudioLink.cginc"
// End Injection INCLUDES from Injection_AudioLink.hlsl ----------------------------------------------------------

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

// Begin Injection UNIFORMS from Injection_Emission_Meta.hlsl ----------------------------------------------------------
TEXTURE2D(_EmissionMap);
// End Injection UNIFORMS from Injection_Emission_Meta.hlsl ----------------------------------------------------------
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
// Begin Injection MATERIAL_CBUFFER from Injection_Emission_CBuffer.hlsl ----------------------------------------------------------
    half  _Emission;
    half4 _EmissionColor;
    half  _EmissionFalloff;
    half  _BakedMutiplier;
// End Injection MATERIAL_CBUFFER from Injection_Emission_CBuffer.hlsl ----------------------------------------------------------
CBUFFER_END

struct appdata
{
    float4 vertex : POSITION;
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv2 : TEXCOORD2;
    float4 uv3 : TEXCOORD3;
// Begin Injection VERTEX_IN from Injection_ALNormMeta.hlsl ----------------------------------------------------------
        half3 normal : NORMAL;
        half4 tangent : TANGENT;
// End Injection VERTEX_IN from Injection_ALNormMeta.hlsl ----------------------------------------------------------
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;

    #ifdef EDITOR_VISUALIZATION
    float4 VizUV : TEXCOORD1;
    float4 LightCoord : TEXCOORD2;
    #endif

// Begin Injection INTERPOLATORS from Injection_ALNormMeta.hlsl ----------------------------------------------------------
    half2x3 TStoWS : TEXCOORD3;
// End Injection INTERPOLATORS from Injection_ALNormMeta.hlsl ----------------------------------------------------------

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

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

v2f vert(appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.vertex = UnityMetaVertexPosition(v.vertex.xyz, v.uv1.xy, v.uv2.xy);
    o.uv = TRANSFORM_TEX(v.uv0.xy, _BaseMap);

    #ifdef EDITOR_VISUALIZATION
        float2 vizUV = 0;
        float4 lightCoord = 0;
        UnityEditorVizData(v.vertex.xyz, v.uv0.xy, v.uv1.xy, v.uv2.xy, vizUV, lightCoord);
        o.VizUV = float4(vizUV, 0, 0);
        o.LightCoord = lightCoord;
    #endif

// Begin Injection VERTEX_END from Injection_ALNormMeta.hlsl ----------------------------------------------------------
    VertexNormalInputs ntb = GetVertexNormalInputs(v.normal, v.tangent);
    o.TStoWS = half2x3(ntb.normalWS.x, ntb.tangentWS.x, ntb.bitangentWS.x, 
        ntb.normalWS.y, ntb.tangentWS.y, ntb.bitangentWS.y
        );
// End Injection VERTEX_END from Injection_ALNormMeta.hlsl ----------------------------------------------------------

    return o;
}

half4 frag(v2f i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    
    MetaInput metaInput = (MetaInput)0;

    float2 uv_main = i.uv;

    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
    metaInput.Albedo = albedo.rgb;

// Begin Injection FRAG_POST_INPUTS from Injection_ALNormMeta.hlsl ----------------------------------------------------------
    half2x3 TStoWS = i.TStoWS;
// End Injection FRAG_POST_INPUTS from Injection_ALNormMeta.hlsl ----------------------------------------------------------

    half4 emission = half4(0, 0, 0, 0);

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
// Begin Injection EMISSION from Injection_Emission_Meta.hlsl ----------------------------------------------------------
    half4 emissionDefault = _EmissionColor * SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, i.uv);
    emissionDefault.rgb *= _BakedMutiplier * _Emission;
    emissionDefault.rgb *= lerp(albedo.rgb, half3(1, 1, 1), emissionDefault.a);
    emission += emissionDefault;
// End Injection EMISSION from Injection_Emission_Meta.hlsl ----------------------------------------------------------
// Begin Injection EMISSION from Injection_ALBakedEm.hlsl ----------------------------------------------------------
        emission.rgb *= 8;
// End Injection EMISSION from Injection_ALBakedEm.hlsl ----------------------------------------------------------

    metaInput.Emission = emission.rgb;

    #ifdef EDITOR_VISUALIZATION
        metaInput.VizUV = i.VizUV.xy;
        metaInput.LightCoord = i.LightCoord;
    #endif

    return MetaFragment(metaInput);
}