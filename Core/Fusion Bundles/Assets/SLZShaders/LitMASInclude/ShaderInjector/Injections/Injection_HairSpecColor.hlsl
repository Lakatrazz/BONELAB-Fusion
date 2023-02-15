
//#!INJECT_BEGIN PRE_LIGHTING_CALC 100
	surfData.specular *= albedo.rgb/Max3(albedo.r,albedo.g,albedo.b);
//#!INJECT_END