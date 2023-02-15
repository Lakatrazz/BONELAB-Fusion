/*-----------------------------------------------------------------------------------------------------*
 *-----------------------------------------------------------------------------------------------------*
 * WARNING: THIS FILE WAS CREATED WITH SHADERINJECTOR, AND SHOULD NOT BE EDITED DIRECTLY. MODIFY THE   *
 * BASE INCLUDE AND INJECTED FILES INSTEAD, AND REGENERATE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!   *
 *-----------------------------------------------------------------------------------------------------*
 *-----------------------------------------------------------------------------------------------------*/

#define SHADERPASS SHADERPASS_DEPTHNORMALS

#if defined(SHADER_API_MOBILE)
#else
#endif


#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/EncodeNormalsTexture.hlsl"

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float4 normalWS : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
// Begin Injection MATERIAL_CBUFFER from Injection_Triplanar_CBuffer.hlsl ----------------------------------------------------------
float4 _DetailMap_ST;
half  _Details;
half  _Normals;
half  _DetailsuseLocalUVs;
half _RotateUVs;
half _UVScaler;
// End Injection MATERIAL_CBUFFER from Injection_Triplanar_CBuffer.hlsl ----------------------------------------------------------
// Begin Injection MATERIAL_CBUFFER from Injection_SSR_CBuffer.hlsl ----------------------------------------------------------
    float _SSRTemporalMul;
// End Injection MATERIAL_CBUFFER from Injection_SSR_CBuffer.hlsl ----------------------------------------------------------
CBUFFER_END
    

v2f vert(appdata v)
{

    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


    o.vertex = TransformObjectToHClip(v.vertex.xyz);

    o.normalWS = float4(TransformObjectToWorldNormal(v.normal), 1);

    return o;
}

half4 frag(v2f i) : SV_Target
{
   UNITY_SETUP_INSTANCE_ID(i);
   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);


   half4 normals = half4(0, 0, 0, 1);

   normals.xyz = half3(NormalizeNormalPerPixel(i.normalWS.xyz));
   normals.xyz = EncodeWSNormalForNormalsTex(normals.xyz);


    return normals;
}