//#!INJECT_BEGIN UNIFORMS 0
TEXTURE2D(_PenMap);
//#!INJECT_END

//#!INJECT_BEGIN MATERIAL_CBUFFER 0
	float4 _PenMap_ST;
	half  _PenMono;
	half4  _PenMonoColor;
//#!INJECT_END

//#!INJECT_BEGIN FRAG_POST_READ 0
	float2 uv_pen = mad(float2(i.uv0XY_bitZ_fog.xy), _PenMap_ST.xy, _PenMap_ST.zw);
	half4 penMap = SAMPLE_TEXTURE2D(_PenMap, sampler_BaseMap, uv_pen);
	penMap = _PenMono > 0.5 ? penMap.rrrr : penMap;
	penMap.rgb = _PenMono > 0.5 ? penMap.rgb * _PenMonoColor.rgb : penMap.rgb;
	albedo.rgb = penMap.rgb + albedo.rgb * (1.0h - penMap.a);
//#!INJECT_END