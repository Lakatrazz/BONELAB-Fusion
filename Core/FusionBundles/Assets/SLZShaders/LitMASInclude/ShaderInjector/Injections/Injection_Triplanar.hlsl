//#!INJECT_BEGIN UNIVERSAL_DEFINES 1
    #pragma multi_compile_local_fragment _ _EXPENSIVE_TP

    #if defined(_EXPENSIVE_TP)
        #define SLZ_SAMPLE_TP_MAIN(tex, sampl, uv) SAMPLE_TEXTURE2D_GRAD(tex, sampl, uv, ddxMain, ddyMain)
        #define SLZ_SAMPLE_TP_DETAIL(tex, sampl, uv) SAMPLE_TEXTURE2D_GRAD(tex, sampl, uv, ddxDetail, ddyDetail)
    #else
        #define SLZ_SAMPLE_TP_MAIN(tex, sampl, uv) SAMPLE_TEXTURE2D(tex, sampl, uv)
        #define SLZ_SAMPLE_TP_DETAIL(tex, sampl, uv) SAMPLE_TEXTURE2D(tex, sampl, uv)
    #endif
//#!INJECT_END


//#!INJECT_BEGIN INCLUDES 0
#include "Include/Triplanar.hlsl"
//#!INJECT_END

//#!INJECT_BEGIN MATERIAL_CBUFFER 0
    float4 _DetailMap_ST;
    half  _Details;
    half  _Normals;
    half  _DetailsuseLocalUVs;
    half _RotateUVs;
    half _UVScaler;
//#!INJECT_END


//#!INJECT_BEGIN VERTEX_NORMALS 0
    o.normXYZ_tanX = half4(TransformObjectToWorldNormal(v.normal, false), 0);
    o.uv0XY_bitZ_fog.z = v.tangent.w; //Avoid optimization that would remove the tangent from the vertex input (causes issues)
//#!INJECT_END



//#!INJECT_BEGIN FRAG_READ_INPUTS 0

        /*-Triplanar---------------------------------------------------------------------------------------------------------*/

        float2 uvTP;
        half3x3 TStoWsTP;
        half2 scale = 1.0/_UVScaler;
        
        #if defined(_EXPENSIVE_TP)
            tpDerivatives tpDD;
            GetDirectionalDerivatives(i.wPos, tpDD);
            half2 ddxTP, ddyTP;
            GetTPUVExpensive(uvTP, ddxTP, ddyTP, TStoWsTP, i.wPos, normalize(i.normXYZ_tanX.xyz), tpDD);
            ddxTP = _RotateUVs ? half2(-ddxTP.y, ddxTP.x) : ddxTP;
            ddyTP = _RotateUVs ? half2(-ddyTP.y, ddyTP.x) : ddyTP;
            half2 ddxMain = ddxTP * scale;
            half2 ddyMain = ddyTP * scale;
        #else
            GetTPUVCheap(uvTP, TStoWsTP, i.wPos, normalize(i.normXYZ_tanX.xyz));
        #endif
        
        uvTP = _RotateUVs ? float2(-uvTP.y, uvTP.x) : uvTP;
        float2 uv_main = mad(uvTP, scale, _BaseMap_ST.zw);
        half4 albedo = SLZ_SAMPLE_TP_MAIN(_BaseMap, sampler_BaseMap, uv_main) * _BaseColor;
        half3 mas = SLZ_SAMPLE_TP_MAIN(_MetallicGlossMap, sampler_BaseMap, uv_main).rgb;

//#!INJECT_END


//#!INJECT_BEGIN NORMAL_MAP 0

        /*-Triplanar Psuedo tangent space normals----------------------------------------------------------------------------*/
        normalMap = SLZ_SAMPLE_TP_MAIN(_BumpMap, sampler_BaseMap, uv_main);
        normalTS = UnpackNormal(normalMap);
        normalTS = _Normals ? normalTS : half3(0, 0, 1);
        normalTS = _RotateUVs ? half3(normalTS.y, -normalTS.x, normalTS.z) : normalTS;
        geoSmooth = _Normals ? normalMap.b : 1.0;
        smoothness = saturate(smoothness + geoSmooth - 1.0);

//#!INJECT_END

//#!INJECT_BEGIN DETAIL_MAP 0
        /*-Triplanar---------------------------------------------------------------------------------------------------------*/
        float2 uv_detail = mad(uvTP, _DetailMap_ST.xx, _DetailMap_ST.zw);
        uv_detail = _DetailsuseLocalUVs ? mad(float2(i.uv0XY_bitZ_fog.xy), _DetailMap_ST.xy, _DetailMap_ST.zw) : uv_detail;
#if defined(_EXPENSIVE_TP)
        half2 ddxDetail = ddx(uv_detail);
        half2 ddyDetail = ddy(uv_detail);
        ddxDetail = _DetailsuseLocalUVs ? ddxDetail : ddxTP * _DetailMap_ST.xx;
        ddyDetail = _DetailsuseLocalUVs ? ddyDetail : ddyTP * _DetailMap_ST.xx;
#endif
        half4 detailMap = SLZ_SAMPLE_TP_DETAIL(_DetailMap, sampler_DetailMap, uv_detail);
        half3 detailTS = half3(2.0 * detailMap.ag - 1.0, 1.0);
        detailTS = _RotateUVs && !(_DetailsuseLocalUVs) ? half3(detailTS.y, -detailTS.x, detailTS.z) : detailTS;
        normalTS = BlendNormal(normalTS, detailTS);
//#!INJECT_END

//#!INJECT_BEGIN NORMAL_TRANSFORM 0

    /*-Triplanar-------------------------------------------------------------------------------------------------------------*/
    half3 normalWS = mul(TStoWsTP, normalTS);
    normalWS = normalize(normalWS);

//#!INJECT_END