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
// Begin Injection VERTEX_IN from Injection_NormalMap_DepthNormals.hlsl ----------------------------------------------------------
	float4 tangent : TANGENT;
    float2 uv0 : TEXCOORD0;
// End Injection VERTEX_IN from Injection_NormalMap_DepthNormals.hlsl ----------------------------------------------------------
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float4 normalWS : NORMAL;
// Begin Injection INTERPOLATORS from Injection_NormalMap_DepthNormals.hlsl ----------------------------------------------------------
    float4 tanYZ_bitXY : TEXCOORD0;
    float4 uv0XY_bitZ_fog : TEXCOORD1;
// End Injection INTERPOLATORS from Injection_NormalMap_DepthNormals.hlsl ----------------------------------------------------------
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

// Begin Injection UNIFORMS from Injection_NormalMap_DepthNormals.hlsl ----------------------------------------------------------
	TEXTURE2D(_BumpMap);
	SAMPLER(sampler_BumpMap);
// End Injection UNIFORMS from Injection_NormalMap_DepthNormals.hlsl ----------------------------------------------------------

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
// Begin Injection MATERIAL_CBUFFER from Injection_NormalMap_CBuffer.hlsl ----------------------------------------------------------
float4 _DetailMap_ST;
half  _Details;
half  _Normals;
// End Injection MATERIAL_CBUFFER from Injection_NormalMap_CBuffer.hlsl ----------------------------------------------------------
// Begin Injection MATERIAL_CBUFFER from Injection_WhiteBoard_CBuffer.hlsl ----------------------------------------------------------
	float4 _PenMap_ST;
	half  _PenMono;
	half4  _PenMonoColor;
// End Injection MATERIAL_CBUFFER from Injection_WhiteBoard_CBuffer.hlsl ----------------------------------------------------------
CBUFFER_END
    

v2f vert(appdata v)
{

    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


    o.vertex = TransformObjectToHClip(v.vertex.xyz);

// Begin Injection VERTEX_NORMAL from Injection_NormalMap_DepthNormals.hlsl ----------------------------------------------------------
	VertexNormalInputs ntb = GetVertexNormalInputs(v.normal, v.tangent);
	o.normalWS = float4(ntb.normalWS, ntb.tangentWS.x);
	o.tanYZ_bitXY = float4(ntb.tangentWS.yz, ntb.bitangentWS.xy);
	o.uv0XY_bitZ_fog.zw = ntb.bitangentWS.zz;
	o.uv0XY_bitZ_fog.xy = TRANSFORM_TEX(v.uv0, _BaseMap);
// End Injection VERTEX_NORMAL from Injection_NormalMap_DepthNormals.hlsl ----------------------------------------------------------

    return o;
}

half4 frag(v2f i) : SV_Target
{
   UNITY_SETUP_INSTANCE_ID(i);
   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);


   half4 normals = half4(0, 0, 0, 1);

// Begin Injection FRAG_NORMALS from Injection_NormalMap_DepthNormals.hlsl ----------------------------------------------------------
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
// End Injection FRAG_NORMALS from Injection_NormalMap_DepthNormals.hlsl ----------------------------------------------------------


    return normals;
}