//#!INJECT_BEGIN UNIFORMS 0
TEXTURE2D(_EmissionMap);
//#!INJECT_END

//#!INJECT_BEGIN EMISSION 10
    half4 emissionDefault = _EmissionColor * SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, i.uv);
    emissionDefault.rgb *= _BakedMutiplier * _Emission;
    emissionDefault.rgb *= lerp(albedo.rgb, half3(1, 1, 1), emissionDefault.a);
    emission += emissionDefault;
//#!INJECT_END