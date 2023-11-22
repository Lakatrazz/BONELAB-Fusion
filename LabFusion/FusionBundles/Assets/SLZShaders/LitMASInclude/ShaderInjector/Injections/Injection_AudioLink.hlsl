//#!INJECT_BEGIN INCLUDES 1
#include "AudioLink/Shaders/AudioLink.cginc"
//#!INJECT_END

//#!INJECT_BEGIN UNIFORMS 1
TEXTURE2D(_AudioLinkMap);
SAMPLER(sampler_AudioLinkMap);
TEXTURE2D(_AudioLinkNoise);
SAMPLER(sampler_AudioLinkNoise);
//#!INJECT_END

//#!INJECT_BEGIN MATERIAL_CBUFFER 0
    half  _AudioInputBoost;
    half  _SmoothstepBlend;
    half  _AudioLinkBaseBlend;
    half4 _LowsColor;
    half4 _MidsColor;
    half4 _HighsColor;
//#!INJECT_END

//#!INJECT_BEGIN FUNCTIONS 0
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

//#!INJECT_END

//#!INJECT_BEGIN EMISSION 0
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

//#!INJECT_END