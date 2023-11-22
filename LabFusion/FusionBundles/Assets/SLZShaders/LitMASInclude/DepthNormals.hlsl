#define SHADERPASS SHADERPASS_DEPTHNORMALS

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/EncodeNormalsTexture.hlsl"

struct appdata
{
    float4 vertex : POSITION;
    half3 normal : NORMAL;
   // half4 tangent : TANGENT;
    //float3 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    //float2 uv : TEXCOORD0;
    half3 normalWS : NORMAL;
    //half3 tangentWS : TANGENT;
    //half3 bitangentWS : BITANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
CBUFFER_END
    
v2f vert(appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    //o.uv = mad(v.uv.xy, _BaseMap_ST.xy, _BaseMap_ST.zw);
    o.vertex = TransformObjectToHClip(v.vertex.xyz);

    //VertexNormalInputs ntb = GetVertexNormalInputs(v.normal, v.tangent);
    o.normalWS = TransformObjectToWorldNormal(v.normal);
    //o.tangentWS = ntb.tangentWS;
    //o.bitangentWS = ntb.bitangentWS;
    return o;
}

/* Just set object normals for now, reading normal map in depth-normals prepass is expensive*/

half4 frag(v2f i) : SV_Target
{
   UNITY_SETUP_INSTANCE_ID(i);
   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
   half3 normals = NormalizeNormalPerPixel(i.normalWS);
   normals = EncodeWSNormalForNormalsTex(normals);
   return half4(normals, 0.0);
}