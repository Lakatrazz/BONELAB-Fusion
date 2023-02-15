//#!INJECT_BEGIN VERTEX_IN 0
float4 color : COLOR;
//#!INJECT_END

//#!INJECT_BEGIN INTERPOLATORS 0
   float4 color : COLOR;
//#!INJECT_END

//#!INJECT_BEGIN VERTEX_END 0
	o.color = v.color;
//#!INJECT_END

//#!INJECT_BEGIN FRAG_POST_READ 0
	_BaseColor = lerp(1, _BaseColor, albedo.a);
//#!INJECT_END

//#!INJECT_BEGIN PRE_FRAGDATA 0
	ao *= i.color.a;
	albedo.rgb *= i.color.rgb;
//#!INJECT_END