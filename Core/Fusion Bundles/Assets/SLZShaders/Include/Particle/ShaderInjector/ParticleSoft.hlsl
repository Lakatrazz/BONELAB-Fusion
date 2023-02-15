/*-----------------------------------------------------------------------------------------------------*
 *-----------------------------------------------------------------------------------------------------*
 * WARNING: THIS FILE WAS CREATED WITH SHADERINJECTOR, AND SHOULD NOT BE EDITED DIRECTLY. MODIFY THE   *
 * BASE INCLUDE AND INJECTED FILES INSTEAD, AND REGENERATE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!   *
 *-----------------------------------------------------------------------------------------------------*
 *-----------------------------------------------------------------------------------------------------*/


#pragma target 5.0
//#pragma multi_compile_fragment _ _VOLUMETRICS_ENABLED

#if defined(SHADER_API_MOBILE)
#else
#endif


#pragma multi_compile_fog
#pragma skip_variants FOG_LINEAR
#pragma multi_compile_fragment _ _VOLUMETRICS_ENABLED
#pragma multi_compile_instancing
#pragma instancing_options procedural:ParticleInstancingSetup



#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Include/Particle/billboard.hlsl"
// Begin Injection INCLUDES from Injection_Particle_SoftBlend.hlsl ----------------------------------------------------------
#if defined(SHADER_API_MOBILE)
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SLZBlueNoise.hlsl"
#else
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#endif
// End Injection INCLUDES from Injection_Particle_SoftBlend.hlsl ----------------------------------------------------------

struct appdata
{
    float4 vertex : POSITION;
    float4 uv0_vertexStream0_xy : TEXCOORD0;
    half4 color : COLOR;
    float vertexStream1 : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
// Begin Injection VERTEX_CENTROID from Injection_Particle_SoftBlend.hlsl ----------------------------------------------------------
#if defined(SHADER_API_MOBILE)
	centroid float4 vertex : SV_POSITION;
#else
	float4 vertex : SV_POSITION;
#endif
// End Injection VERTEX_CENTROID from Injection_Particle_SoftBlend.hlsl ----------------------------------------------------------
    float2 uv0 : TEXCOORD0;
    float4 wPos_xyz_fog_x : TEXCOORD1;
    half4 color : COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

struct fragOut
{
    half4 color : SV_Target;
// Begin Injection OUTPUT_SEMANTICS from Injection_Particle_SoftBlend.hlsl ----------------------------------------------------------
#if defined(SHADER_API_MOBILE)
	float depth : SV_DepthLessEqual;
#endif
// End Injection OUTPUT_SEMANTICS from Injection_Particle_SoftBlend.hlsl ----------------------------------------------------------
};

//#include "Include/PlatformSamplers.hlsl"

TEXTURE2D(_BaseMap);
SamplerState sampler_BaseMap;


CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
// Begin Injection MATERIAL_CBUFFER from Injection_Particle_SoftBlend.hlsl ----------------------------------------------------------
	float _SoftFactor;
// End Injection MATERIAL_CBUFFER from Injection_Particle_SoftBlend.hlsl ----------------------------------------------------------
CBUFFER_END


v2f vert(appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


    float3 particleCenter = float3(v.uv0_vertexStream0_xy.zw, v.vertexStream1.x);
    v.vertex.xyz = ParticleFaceCamera(v.vertex.xyz, particleCenter);
    o.wPos_xyz_fog_x.xyz = TransformObjectToWorld(v.vertex.xyz);
    o.vertex = TransformWorldToHClip(o.wPos_xyz_fog_x.xyz);
    o.uv0 = TRANSFORM_TEX(v.uv0_vertexStream0_xy.xy, _BaseMap);
    half clipZ_0Far = UNITY_Z_0_FAR_FROM_CLIPSPACE(o.vertex.z);
    o.wPos_xyz_fog_x.w = unity_FogParams.x * clipZ_0Far;
    o.color = v.color;


    return o;
}

fragOut frag(v2f i)
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    fragOut f;

// Begin Injection FRAG_BEGIN from Injection_Particle_screenspaceUVs.hlsl ----------------------------------------------------------
		float2 screenUVs = GetNormalizedScreenSpaceUV(i.vertex);
// End Injection FRAG_BEGIN from Injection_Particle_screenspaceUVs.hlsl ----------------------------------------------------------

    f.color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv0);
    f.color *= _BaseColor * i.color;
    
    half3 viewDir = normalize(half3(i.wPos_xyz_fog_x.xyz - _WorldSpaceCameraPos));
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
        #if defined(SLZ_PARTICLE_ADDITIVE)
            f.color.rgb *= 1.0 - ComputeFogIntensity(i.wPos_xyz_fog_x.w);
        #elif defined(SLZ_PARTICLE_MULTIPLICATIVE)
            f.color.rgb = lerp(f.color.rgb, half3(1,1,1), ComputeFogIntensity(i.wPos_xyz_fog_x.w));
        #elif defined(SLZ_PARTICLE_ALPHABLEND)
            f.color.rgb = MixFog(f.color.rgb, viewDir, i.wPos_xyz_fog_x.w);
        #endif
    #endif


    #if defined(SLZ_PARTICLE_ADDITIVE)
        #if defined(_VOLUMETRICS_ENABLED)
        f.color *= GetVolumetricColor(i.wPos_xyz_fog_x.xyz).a;
        #endif
    #elif defined(SLZ_PARTICLE_ALPHABLEND)
        f.color = Volumetrics(f.color, i.wPos_xyz_fog_x.xyz);
    #endif

// Begin Injection FRAG_END from Injection_Particle_SoftBlend.hlsl ----------------------------------------------------------
		#if defined(SHADER_API_MOBILE)
			half noise = GetScreenNoiseR(screenUVs);
			float depthOffset = _SoftFactor * noise * noise ;
			float vertDepth = i.vertex.z;
			float cameraZ = 1.0 / (_ZBufferParams.z * vertDepth + _ZBufferParams.w);
			cameraZ += depthOffset;
			f.depth = (rcp(cameraZ) - _ZBufferParams.w) / _ZBufferParams.z;
		#else
			float rawDepth = SampleSceneDepth(screenUVs);
			float sceneZ = (unity_OrthoParams.w == 0) ? LinearEyeDepth(rawDepth, _ZBufferParams) : LinearDepthToEyeDepth(rawDepth);
			float thisZ = LinearEyeDepth(i.wPos_xyz_fog_x.xyz, GetWorldToViewMatrix());
			float fade = saturate((1 / _SoftFactor) * (sceneZ - thisZ));
			fade *= fade;
			#if defined(SLZ_PARTICLE_ADDITIVE)
				f.color *= fade;
			#elif defined(SLZ_PARTICLE_MULTIPLICATIVE)
				f.color = lerp(fade, float4(1,1,1,1), f.color));
			#else
				f.color.a *= fade;
			#endif
		#endif
// End Injection FRAG_END from Injection_Particle_SoftBlend.hlsl ----------------------------------------------------------

     return f;
}