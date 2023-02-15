#define SHADERPASS SHADERPASS_DEPTHNORMALS

#if defined(SHADER_API_MOBILE)
    //#!INJECT_POINT MOBILE_DEFINES
#else
    //#!INJECT_POINT STANDALONE_DEFINES
#endif

//#!INJECT_POINT UNIVERSAL_DEFINES

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/EncodeNormalsTexture.hlsl"
//#!INJECT_POINT INCLUDES

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    //#!INJECT_POINT VERTEX_IN
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float4 normalWS : NORMAL;
    //#!INJECT_POINT INTERPOLATORS
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

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

    //#!INJECT_POINT VERTEX_BEGIN

    //#!INJECT_POINT VERTEX_POSITION
    //#!INJECT_DEFAULT
    o.vertex = TransformObjectToHClip(v.vertex.xyz);
    //#!INJECT_END

    //#!INJECT_POINT VERTEX_NORMAL
    //#!INJECT_DEFAULT
    o.normalWS = float4(TransformObjectToWorldNormal(v.normal), 1);
    //#!INJECT_END

    //#!INJECT_POINT VERTEX_END
    return o;
}

half4 frag(v2f i) : SV_Target
{
   UNITY_SETUP_INSTANCE_ID(i);
   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

   //#!INJECT_POINT FRAG_BEGIN

   half4 normals = half4(0, 0, 0, 1);

   //#!INJECT_POINT FRAG_NORMALS
   //#!INJECT_DEFAULT
   normals.xyz = half3(NormalizeNormalPerPixel(i.normalWS.xyz));
   normals.xyz = EncodeWSNormalForNormalsTex(normals.xyz);
   //#!INJECT_END

   //#!INJECT_POINT FRAG_END

    return normals;
}