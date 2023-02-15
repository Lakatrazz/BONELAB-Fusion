//#!INJECT_BEGIN FRAG_READ_INPUTS 0
	float2 uv_main = mad(float2(i.uv0XY_bitZ_fog.xy), _BaseMap_ST.xy, _BaseMap_ST.zw);
	float2 uv_detail = mad(float2(i.uv0XY_bitZ_fog.xy), _DetailMap_ST.xy, _DetailMap_ST.zw);
	half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv_main) * _BaseColor;
	half3 mas = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_BaseMap, uv_main).rgb;
//#!INJECT_END