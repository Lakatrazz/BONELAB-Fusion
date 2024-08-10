//#!INJECT_BEGIN INCLUDES 0
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SLZBlueNoise.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SLZSoftBlend.hlsl"
//#!INJECT_END

//#!INJECT_BEGIN MATERIAL_CBUFFER 0
	float _SoftFactor;
//#!INJECT_END

//#!INJECT_BEGIN VERTEX_CENTROID 0
#if defined(SHADER_API_MOBILE)
	centroid float4 vertex : SV_POSITION;
#else
	float4 vertex : SV_POSITION;
#endif
//#!INJECT_END

//#!INJECT_BEGIN OUTPUT_SEMANTICS 0
#if defined(SHADER_API_MOBILE)
	float depth : SV_DepthLessEqual;
#endif
//#!INJECT_END


//#!INJECT_BEGIN FRAG_END 99
		#if defined(SHADER_API_MOBILE)
			half noise = GetScreenNoiseR(screenUVs);
			f.depth = SLZSoftBlendZTest(i.vertex.z, noise, _SoftFactor);
		#else
			float rawDepth = SampleSceneDepth(screenUVs);
			float viewZ = dot(GetWorldToViewMatrix()._m20_m21_m22_m23, float4(i.wPos_xyz_fog_x.xyz, 1));
			float fade = SLZSoftBlendDepth(rawDepth, viewZ, _SoftFactor);
			#if defined(SLZ_PARTICLE_ADDITIVE)
				f.color *= fade;
			#elif defined(SLZ_PARTICLE_MULTIPLICATIVE)
				f.color = lerp(fade, float4(1,1,1,1), f.color));
			#else
				f.color.a *= fade;
			#endif
		#endif
//#!INJECT_END