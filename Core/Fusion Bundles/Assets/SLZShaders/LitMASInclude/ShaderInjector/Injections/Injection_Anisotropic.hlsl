//#!INJECT_BEGIN UNIVERSAL_DEFINES 1
#define _SLZ_ANISO_SPECULAR
//#!INJECT_END

//#!INJECT_BEGIN MATERIAL_CBUFFER 1
	half _AnisoAspect;
//#!INJECT_END

//#!INJECT_BEGIN PRE_LIGHTING_CALC 1
	SLZSurfDataAddAniso(surfData, _AnisoAspect);
	SLZFragDataAddAniso(fragData, TStoWS._m00_m10_m20, TStoWS._m01_m11_m21, surfData.roughnessT, surfData.roughnessB);
//#!INJECT_END