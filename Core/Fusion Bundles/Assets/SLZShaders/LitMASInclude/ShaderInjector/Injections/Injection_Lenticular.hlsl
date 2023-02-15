//#!INJECT_BEGIN FUNCTIONS 0 
float function1()
//#!INJECT_END

//#!INJECT_BEGIN PRE_FRAGDATA 0
	ao *= i.color.a;
	albedo *= i.color.rgb;
//#!INJECT_END