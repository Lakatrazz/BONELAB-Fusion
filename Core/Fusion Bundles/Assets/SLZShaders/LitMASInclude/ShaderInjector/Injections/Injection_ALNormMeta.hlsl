
//#!INJECT_BEGIN VERTEX_IN 4
        half3 normal : NORMAL;
        half4 tangent : TANGENT;
//#!INJECT_END

//#!INJECT_BEGIN INTERPOLATORS 4
    //#!TEXCOORD half2x3 TStoWS 1
//#!INJECT_END

//#!INJECT_BEGIN VERTEX_END 0
    VertexNormalInputs ntb = GetVertexNormalInputs(v.normal, v.tangent);
    o.TStoWS = half2x3(ntb.normalWS.x, ntb.tangentWS.x, ntb.bitangentWS.x, 
        ntb.normalWS.y, ntb.tangentWS.y, ntb.bitangentWS.y
        );
//#!INJECT_END

//#!INJECT_BEGIN FRAG_POST_INPUTS 0
    half2x3 TStoWS = i.TStoWS;
//#!INJECT_END