//#!INJECT_BEGIN UNIFORMS 0
	TEXTURE2D(_ScreenSpacePattern);
	SAMPLER(sampler_ScreenSpacePattern);
//#!INJECT_END

//#!INJECT_BEGIN MATERIAL_CBUFFER 0
	half _Speed;
	half _SecondarySpeed;
	float _ScreenSpaceSize;
	float _ParticleOffset;
//#!INJECT_END

//#!INJECT_BEGIN FUNCTIONS 0
	float2 RotateTorchUVs(float2 uv, float2 center, float angle)
	{
		uv = uv - center;
		float sin1, cos1;
		sincos(angle, sin1, cos1);
		float2x2 rotMat = float2x2(cos1, -sin1, sin1, cos1);
		uv = mul(rotMat, uv);
		return uv + center;
	}
//#!INJECT_END

//#!INJECT_BEGIN VERT_BEGIN 0
	v.vertex.z += _ParticleOffset;
//#!INJECT_END

//#!INJECT_BEGIN FRAG_COLOR 0
	float angle1 = frac(_Time[0] * 3.1830988618 * _Speed) * TWO_PI;
	float angle2 = frac(_Time[0] * 3.1830988618 * _Speed * _SecondarySpeed) * TWO_PI;
	angle1 = -angle1;
	float2 uv1 = RotateTorchUVs(i.uv0.xy, float2(0.5, 0.5), angle1);
	half layer1 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv1).r;
	float2 uv2 = RotateTorchUVs(i.uv0.xy, float2(0.5, 0.5), angle2);
	half layer2 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv2).r;
	half4 SSPattern = SAMPLE_TEXTURE2D(_ScreenSpacePattern, sampler_ScreenSpacePattern, _ScreenParams.xy * screenUVs / _ScreenSpaceSize);
	f.color = (layer1 * layer2 * i.color.a) * SSPattern * _BaseColor * i.color;
//#!INJECT_END