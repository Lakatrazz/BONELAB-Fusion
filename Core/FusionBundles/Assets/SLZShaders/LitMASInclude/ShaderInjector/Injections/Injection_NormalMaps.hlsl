
//#!INJECT_BEGIN INTERPOLATORS 4
	//#!TEXCOORD half4 tanYZ_bitXY 1
//#!INJECT_END

//#!INJECT_BEGIN VERTEX_NORMALS 0
	VertexNormalInputs ntb = GetVertexNormalInputs(v.normal, v.tangent);
	o.normXYZ_tanX = half4(ntb.normalWS, ntb.tangentWS.x);
	o.tanYZ_bitXY = half4(ntb.tangentWS.yz, ntb.bitangentWS.xy);
	o.uv0XY_bitZ_fog.z = ntb.bitangentWS.z;
//#!INJECT_END

//#!INJECT_BEGIN NORMAL_MAP 0
	normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BaseMap, uv_main);
	normalTS = UnpackNormal(normalMap);
	normalTS = _Normals ? normalTS : half3(0, 0, 1);
	geoSmooth = _Normals ? normalMap.b : 1.0;
	smoothness = saturate(smoothness + geoSmooth - 1.0);
//#!INJECT_END

//#!INJECT_BEGIN DETAIL_MAP 0
		half4 detailMap = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, uv_detail);
		half3 detailTS = half3(2.0 * detailMap.ag - 1.0, 1.0);
		normalTS = BlendNormal(normalTS, detailTS);
//#!INJECT_END

//#!INJECT_BEGIN NORMAL_TRANSFORM 0
	half3 normalWS = i.normXYZ_tanX.xyz;
	half3x3 TStoWS = half3x3(
		i.normXYZ_tanX.w, i.tanYZ_bitXY.z, normalWS.x,
		i.tanYZ_bitXY.x, i.tanYZ_bitXY.w, normalWS.y,
		i.tanYZ_bitXY.y, i.uv0XY_bitZ_fog.z, normalWS.z
		);
	normalWS = mul(TStoWS, normalTS);
	normalWS = normalize(normalWS);
//#!INJECT_END

//#!INJECT_BEGIN SPEC_AA 0
	#if !defined(SHADER_API_MOBILE) && !defined(LITMAS_FEATURE_TP) // Specular antialiasing based on normal derivatives. Only on PC to avoid cost of derivatives on Quest
		smoothness = min(smoothness, SLZGeometricSpecularAA(normalWS));
	#endif
//#!INJECT_END