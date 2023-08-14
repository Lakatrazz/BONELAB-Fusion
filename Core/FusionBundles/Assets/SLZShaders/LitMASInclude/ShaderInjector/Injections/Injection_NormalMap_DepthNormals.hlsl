//#!INJECT_BEGIN VERTEX_IN 0
	float4 tangent : TANGENT;
	//#!TEXCOORD float2 uv0 0
//#!INJECT_END

//#!INJECT_BEGIN UNIFORMS 0
	TEXTURE2D(_BumpMap);
	SAMPLER(sampler_BumpMap);
//#!INJECT_END

//#!INJECT_BEGIN INTERPOLATORS 4
	//#!TEXCOORD float4 tanYZ_bitXY 1
	//#!TEXCOORD float4 uv0XY_bitZ_fog 1
//#!INJECT_END

//#!INJECT_BEGIN VERTEX_NORMAL 0
	VertexNormalInputs ntb = GetVertexNormalInputs(v.normal, v.tangent);
	o.normalWS = float4(ntb.normalWS, ntb.tangentWS.x);
	o.tanYZ_bitXY = float4(ntb.tangentWS.yz, ntb.bitangentWS.xy);
	o.uv0XY_bitZ_fog.zw = ntb.bitangentWS.zz;
	o.uv0XY_bitZ_fog.xy = TRANSFORM_TEX(v.uv0, _BaseMap);
//#!INJECT_END

//#!INJECT_BEGIN FRAG_NORMALS 0
	half4 normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv0XY_bitZ_fog.xy);
	half3 normalTS = UnpackNormal(normalMap);
	normalTS = _Normals ? normalTS : half3(0, 0, 1);


	half3x3 TStoWS = half3x3(
		i.normalWS.w, i.tanYZ_bitXY.z, i.normalWS.x,
		i.tanYZ_bitXY.x, i.tanYZ_bitXY.w, i.normalWS.y,
		i.tanYZ_bitXY.y, i.uv0XY_bitZ_fog.z, i.normalWS.z
		);
	half3 normalWS = mul(TStoWS, normalTS);
	normalWS = normalize(normalWS);

	normals = half4(EncodeWSNormalForNormalsTex(normalWS),0);
//#!INJECT_END
