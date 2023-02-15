float3 centerEyePos()
{
	#if defined(UNITY_STEREO_MULTIVIEW_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED) 
		return 0.5 * (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]);
	#else
		return _WorldSpaceCameraPos;
	#endif
}

/** Rotates a particle to face the camera in a more vr-correct way than unity's default particle system
 *
 */

float3 particle_face_camera(float3 vertex, inout float3 normal, inout float3 center)
{
	vertex.xyz -= center;

	float3 head = centerEyePos();
	float3 centerToEye = normalize(center-head);
	float c2eXZLen = length(centerToEye.xz);
	float sin1 = -centerToEye.y;
	float cos1 = c2eXZLen;
	float2x2 rotPitch = float2x2(cos1, sin1, -sin1, cos1);
	
	vertex.zy = mul(rotPitch, vertex.zy);
	normal.zy = mul(rotPitch, normal.zy);
	//tangent.zy = mul(rotPitch, tangent.zy);

	float sin2 = centerToEye.x/c2eXZLen;
	float cos2 = centerToEye.z/c2eXZLen;
	float2x2 rotYaw = float2x2(cos2, sin2, -sin2, cos2);

	vertex.xz = mul(rotYaw,vertex.xz);
	normal.xz = mul(rotYaw,normal.xz);
	//tangent.xz = mul(rotYaw,tangent.xz);

	vertex.xyz += center;
	return vertex;
}




float3 ParticleFaceCamera(float3 vertex, float3 center)
{
    vertex.xyz -= center;

    float3 head = centerEyePos();
    float3 centerToEye = normalize(center - head);
    float c2eXZLen = length(centerToEye.xz);
    float sin1 = -centerToEye.y;
    float cos1 = c2eXZLen;
    float2x2 rotPitch = float2x2(cos1, sin1, -sin1, cos1);
	
    vertex.zy = mul(rotPitch, vertex.zy);


    float sin2 = centerToEye.x / c2eXZLen;
    float cos2 = centerToEye.z / c2eXZLen;
    float2x2 rotYaw = float2x2(cos2, sin2, -sin2, cos2);

    vertex.xz = mul(rotYaw, vertex.xz);


    vertex.xyz += center;
    return vertex;
}

float3 MeshFaceCamera(float3 vertex)
{
	float3 center = float3(0,0,0);
	//vertex = TransformObjectToWorld(vertex);

	float3 head = TransformWorldToObject(centerEyePos());
	float3 centerToEye = normalize(center - head);
	float c2eXZLen = length(centerToEye.xz);
	float sin1 = -centerToEye.y;
	float cos1 = c2eXZLen;
	float2x2 rotPitch = float2x2(cos1, sin1, -sin1, cos1);

	vertex.zy = mul(rotPitch, vertex.zy);


	float sin2 = centerToEye.x / c2eXZLen;
	float cos2 = centerToEye.z / c2eXZLen;
	float2x2 rotYaw = float2x2(cos2, sin2, -sin2, cos2);

	vertex.xz = mul(rotYaw, vertex.xz);


	//vertex.xyz += center;
	return vertex;
}

