
#pragma target 5.0
//#pragma multi_compile_fragment _ _VOLUMETRICS_ENABLED

#if defined(SHADER_API_MOBILE)
    //#!INJECT_POINT MOBILE_DEFINES
#else
    //#!INJECT_POINT STANDALONE_DEFINES
#endif

//#!INJECT_POINT UNIVERSAL_DEFINES

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
//#!INJECT_POINT INCLUDES

struct appdata
{
    float4 vertex : POSITION;
    float4 uv0_vertexStream0_xy : TEXCOORD0;
    half4 color : COLOR;
    //#!INJECT_POINT VERTEX_IN
    //#!INJECT_DEFAULT
    float vertexStream1 : TEXCOORD1;
    //#!INJECT_END
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    //#!INJECT_POINT VERTEX_CENTROID
    //#!INJECT_DEFAULT
    float4 vertex : SV_POSITION;
    //#!INJECT_END
    float2 uv0 : TEXCOORD0;
    float4 wPos_xyz_fog_x : TEXCOORD1;
    half4 color : COLOR;
    //#!INJECT_POINT INTERPOLATORS

    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

struct fragOut
{
    half4 color : SV_Target;
    //#!INJECT_POINT OUTPUT_SEMANTICS
};

//#include "Include/PlatformSamplers.hlsl"

TEXTURE2D(_BaseMap);
SamplerState sampler_BaseMap;

//#!INJECT_POINT UNIFORMS

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
//#!INJECT_POINT MATERIAL_CBUFFER
CBUFFER_END

//#!INJECT_POINT FUNCTIONS

v2f vert(appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    //#!INJECT_POINT VERT_BEGIN

    //#!INJECT_POINT VERT_TRANSFORM
    //#!INJECT_DEFAULT
    float3 particleCenter = float3(v.uv0_vertexStream0_xy.zw, v.vertexStream1.x);
    v.vertex.xyz = ParticleFaceCamera(v.vertex.xyz, particleCenter);
    //#!INJECT_END
    o.wPos_xyz_fog_x.xyz = TransformObjectToWorld(v.vertex.xyz);
    o.vertex = TransformWorldToHClip(o.wPos_xyz_fog_x.xyz);
    o.uv0 = TRANSFORM_TEX(v.uv0_vertexStream0_xy.xy, _BaseMap);
    half clipZ_0Far = UNITY_Z_0_FAR_FROM_CLIPSPACE(o.vertex.z);
    o.wPos_xyz_fog_x.w = unity_FogParams.x * clipZ_0Far;
    o.color = v.color;

    //#!INJECT_POINT VERT_END

    return o;
}

fragOut frag(v2f i)
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    fragOut f;

    //#!INJECT_POINT FRAG_BEGIN

    //#!INJECT_POINT FRAG_COLOR
    //#!INJECT_DEFAULT
    f.color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv0);
    f.color *= _BaseColor * i.color;
    //#!INJECT_END
    
    //#!INJECT_POINT FRAG_FOG
    //#!INJECT_DEFAULT
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
    //#!INJECT_END


    //#!INJECT_POINT FRAG_VOLUMETRICS
    //#!INJECT_DEFAULT
    #if defined(SLZ_PARTICLE_ADDITIVE)
        #if defined(_VOLUMETRICS_ENABLED)
        f.color *= GetVolumetricColor(i.wPos_xyz_fog_x.xyz).a;
        #endif
    #elif defined(SLZ_PARTICLE_ALPHABLEND)
        f.color = Volumetrics(f.color, i.wPos_xyz_fog_x.xyz);
    #endif
    //#!INJECT_END

    //#!INJECT_POINT FRAG_END

     return f;
}