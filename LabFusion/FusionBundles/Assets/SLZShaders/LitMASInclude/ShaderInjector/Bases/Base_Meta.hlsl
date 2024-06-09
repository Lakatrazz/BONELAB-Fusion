#define SHADERPASS SHADERPASS_META
#define PASS_META

#if defined(SHADER_API_MOBILE)

//#!INJECT_POINT MOBILE_DEFINES

#else

//#!INJECT_POINT STANDALONE_DEFINES

#endif

#pragma shader_feature _ EDITOR_VISUALIZATION

//#!INJECT_POINT UNIVERSAL_DEFINES

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

//#!INJECT_POINT INCLUDES

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

//#!INJECT_POINT UNIFORMS

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    //#!INJECT_POINT MATERIAL_CBUFFER
CBUFFER_END

struct appdata
{
    float4 vertex : POSITION;
    //#!TEXCOORD float4 uv0 0
    //#!TEXCOORD float4 uv1 0
    //#!TEXCOORD float4 uv2 0
    //#!TEXCOORD float4 uv3 0
    //#!INJECT_POINT VERTEX_IN
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    //#!TEXCOORD float2 uv 1

    #ifdef EDITOR_VISUALIZATION
    //#!TEXCOORD float4 VizUV 1
    //#!TEXCOORD float4 LightCoord 1
    #endif

    //#!INJECT_POINT INTERPOLATORS

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

//#!INJECT_POINT FUNCTIONS

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

    //#!INJECT_POINT VERTEX_END

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

    //#!INJECT_POINT FRAG_POST_INPUTS

    half4 emission = half4(0, 0, 0, 0);

    //#!INJECT_POINT EMISSION

    metaInput.Emission = emission.rgb;

    #ifdef EDITOR_VISUALIZATION
        metaInput.VizUV = i.VizUV.xy;
        metaInput.LightCoord = i.LightCoord;
    #endif

    return MetaFragment(metaInput);
}