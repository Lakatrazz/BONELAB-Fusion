/*-----------------------------------------------------------------------------------------------------*
 *-----------------------------------------------------------------------------------------------------*
 * WARNING: THIS FILE WAS CREATED WITH SHADERINJECTOR, AND SHOULD NOT BE EDITED DIRECTLY. MODIFY THE   *
 * BASE INCLUDE AND INJECTED FILES INSTEAD, AND REGENERATE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!   *
 *-----------------------------------------------------------------------------------------------------*
 *-----------------------------------------------------------------------------------------------------*/

#define SHADERPASS SHADERPASS_META
#define PASS_META

#if defined(SHADER_API_MOBILE)


#else


#endif

#pragma shader_feature _ EDITOR_VISUALIZATION


#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"


TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);


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

struct appdata
{
    float4 vertex : POSITION;
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv2 : TEXCOORD2;
    float4 uv3 : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;

    #ifdef EDITOR_VISUALIZATION
    float4 VizUV : TEXCOORD1;
    float4 LightCoord : TEXCOORD2;
    #endif


    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


v2f vert(appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.vertex = UnityMetaVertexPosition(v.vertex.xyz, v.uv1.xy, v.uv2.xy);
    o.uv = TRANSFORM_TEX(v.uv0.xy, _BaseMap);

    #ifdef EDITOR_VISUALIZATION
        float2 vizUV = 0;
        float4 lightCoord = 0;
        UnityEditorVizData(v.vertex.xyz, v.uv0.xy, v.uv1.xy, v.uv2.xy, vizUV, lightCoord);
        o.VizUV = float4(vizUV, 0, 0);
        o.LightCoord = lightCoord;
    #endif


    return o;
}

half4 frag(v2f i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    
    MetaInput metaInput = (MetaInput)0;

    float2 uv_main = i.uv;

    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
    metaInput.Albedo = albedo.rgb;


    half4 emission = half4(0, 0, 0, 0);


    metaInput.Emission = emission.rgb;

    #ifdef EDITOR_VISUALIZATION
        metaInput.VizUV = i.VizUV.xy;
        metaInput.LightCoord = i.LightCoord;
    #endif

    return MetaFragment(metaInput);
}