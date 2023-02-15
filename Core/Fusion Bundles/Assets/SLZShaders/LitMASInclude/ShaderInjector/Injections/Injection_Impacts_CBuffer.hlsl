//#!INJECT_BEGIN INCLUDES 0
#include "LitMASInclude/PosespaceImpacts.hlsl"
//#!INJECT_END

//#!INJECT_BEGIN MATERIAL_CBUFFER 0
	float4x4 EllipsoidPosArray[HitArrayCount];
	int _NumberOfHits;
	float4 _HitColor;
//#!INJECT_END