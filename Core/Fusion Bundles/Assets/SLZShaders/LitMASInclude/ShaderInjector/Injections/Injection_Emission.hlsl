//#!INJECT_BEGIN UNIFORMS 0
TEXTURE2D(_EmissionMap);
//#!INJECT_END

//#!INJECT_BEGIN MATERIAL_CBUFFER 0
    half  _Emission;
    half4 _EmissionColor;
    half  _EmissionFalloff;
    half  _BakedMutiplier;
//#!INJECT_END

//#!INJECT_BEGIN EMISSION 10
    UNITY_BRANCH if (_Emission)
    {
        emission += SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, uv_main) * _EmissionColor;
        emission.rgb *= lerp(albedo.rgb, half3(1, 1, 1), emission.a);
        emission.rgb *= pow(abs(fragData.NoV), _EmissionFalloff);
    }
//#!INJECT_END