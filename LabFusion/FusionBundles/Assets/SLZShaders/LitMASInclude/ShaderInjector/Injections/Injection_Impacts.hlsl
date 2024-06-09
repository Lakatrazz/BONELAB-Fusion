//#!INJECT_BEGIN INCLUDES 0
#include "LitMASInclude/PosespaceImpacts.hlsl"
//#!INJECT_END

//#!INJECT_BEGIN INTERPOLATORS 6
//#!TEXCOORD float3 unskinnedObjPos 1
//#!INJECT_END

//#!INJECT_BEGIN MATERIAL_CBUFFER 0
	float4x4 EllipsoidPosArray[HitArrayCount];
	int _NumberOfHits;
	float4 _HitColor;
//#!INJECT_END

//#!INJECT_BEGIN VERTEX_END 1
	o.unskinnedObjPos = v.uv1.xyz;
//#!INJECT_END


//#!INJECT_BEGIN FRAG_POST_INPUTS 0
    half2 impactUV = GetClosestImpactUV(i.unskinnedObjPos, EllipsoidPosArray, _NumberOfHits);
    half4 impactMASI = SampleHitTexture(impactUV);
    impactMASI.a = impactUV.x > 0.999 ? 0 : impactMASI.a;
    albedo = lerp(albedo, _HitColor, impactMASI.a);
    metallic = lerp(metallic, impactMASI.r, impactMASI.a);
    smoothness = lerp(smoothness, impactMASI.b, impactMASI.a);
    ao = min(ao, max(1.0 - impactMASI.a, impactMASI.g));
//#!INJECT_END