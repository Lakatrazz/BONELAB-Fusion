#if !defined(SLZ_TRIPLANAR_INCLUDED)
#define SLZ_TRIPLANAR_INCLUDED
//#define TEXTURE2D_PARAM(textureName, samplerName) Texture2D textureName, sampler samplerName

/**
 * Struct containg the ddx and ddy of the uvs combined into one half4 for each axis
 */
struct tpDerivatives
{
    half4 ddX;
    half4 ddY;
    half4 ddZ;
};

/**
 * Gets the derivatives of the worldspace coordinates reduced to 3 2 dimensional planes defined by each axis
 * 
 * @param      wPos Worldspace position
 * @param[out] dd   struct containing the x and y derivatives of each plane
 */
void GetDirectionalDerivatives(float3 wPos, out tpDerivatives dd)
{
    dd.ddX.xy = ddx(wPos.zy);
    dd.ddY.xy = ddx(wPos.xz);
    dd.ddZ.xy = ddx(wPos.xy);
    dd.ddX.zw = ddy(wPos.zy);
    dd.ddY.zw = ddy(wPos.xz);
    dd.ddZ.zw = ddy(wPos.xy);
}
 
/**
 * Determines the worldspace axis that the given mesh normal is most closely aligned to, and returns the coordinates of the
 * pixel in the plane aligned with that axis and the derivatives associated with that plane. Also generates a tangent to world
 * matrix using the axes orthogonal to the dominant axis as the tangent and bitangent.
 *
 * @param[out] triplanarUV  Coordinates of the fragment in the plane aligned with the dominant axis
 * @param[out] ddxMax       X derivatives of the coordinates of the fragment in the plane aligned with the dominant axis
 * @param[out] ddyMax       Y derivatives of the coordinates of the fragment in the plane aligned with the dominant axis
 * @param[out] tanToWrld    Tangent to world matrix composed of the other two axes as tangent and bitangent, and the mesh normal as the normal
 * @param      wPos         World-space position of the fragment
 * @param      wNorm        World-space mesh normal
 * @param      dd           derivatives of the fragment's coordinates in each axis aligned plane
 */
void GetTPUVExpensive(out float2 triplanarUV, out half2 ddxMax, out half2 ddyMax, out half3x3 tanToWrld, float3 wPos, half3 wNorm,
                             tpDerivatives dd)
{
    half3 dir;
    dir.x = abs(wNorm.x) > abs(wNorm.y) && abs(wNorm.x) > abs(wNorm.z) ? 1 : 0;
    dir.y = abs(wNorm.y) >= abs(wNorm.x) && abs(wNorm.y) > abs(wNorm.z) ? 1 : 0;
    dir.z = abs(wNorm.z) >= abs(wNorm.y) && abs(wNorm.z) >= abs(wNorm.x) ? 1 : 0;
    half3 dirSign = sign(wNorm);
    dirSign.z = -dirSign.z; // u should be flipped on Z
    ddxMax = dir.x * dd.ddX.xy + dir.y * dd.ddY.xy + dir.z * dd.ddZ.xy;
    ddyMax = dir.x * dd.ddX.zw + dir.y * dd.ddY.zw + dir.z * dd.ddZ.zw;
    //half3 maxDir = half3(dirX, dirY, dirZ);
    float2 uvX = wPos.zy * float2(dirSign.x, 1.0);
    triplanarUV = dir.x * uvX;
    float2 uvY = wPos.xz * float2(dirSign.y, 1.0);
    triplanarUV = mad(dir.y, uvY, triplanarUV);
    float2 uvZ = wPos.xy * float2(dirSign.z, 1.0);
    triplanarUV = mad(dir.z, uvZ, triplanarUV);
    
    
    tanToWrld = half3x3(
    dir.y * dirSign.y + dir.z * dirSign.z, 0,             wNorm.x,
    0,                                     dir.x + dir.z, wNorm.y,
    dir.x * dirSign.x,                     dir.y,         wNorm.z
    );
    
}

void GetTPUVCheap(out float2 triplanarUV, out half3x3 tanToWrld, float3 wPos, half3 wNorm)
{
    half3 dir;
    dir.x = abs(wNorm.x) > abs(wNorm.y) && abs(wNorm.x) > abs(wNorm.z) ? 1 : 0;
    dir.y = abs(wNorm.y) >= abs(wNorm.x) && abs(wNorm.y) > abs(wNorm.z) ? 1 : 0;
    dir.z = abs(wNorm.z) >= abs(wNorm.y) && abs(wNorm.z) >= abs(wNorm.x) ? 1 : 0;
    half3 dirSign = sign(wNorm);
    dirSign.z = -dirSign.z; // u should be flipped on Z
    //half3 maxDir = half3(dirX, dirY, dirZ);
    float2 uvX = wPos.zy * float2(dirSign.x, 1.0);
    triplanarUV = dir.x * uvX;
    float2 uvY = wPos.xz * float2(dirSign.y, 1.0);
    triplanarUV = mad(dir.y, uvY, triplanarUV);
    float2 uvZ = wPos.xy * float2(dirSign.z, 1.0);
    triplanarUV = mad(dir.z, uvZ, triplanarUV);

    tanToWrld = half3x3(
        dir.y * dirSign.y + dir.z * dirSign.z, 0, wNorm.x,
        0, dir.x + dir.z, wNorm.y,
        dir.x * dirSign.x, dir.y, wNorm.z
        );
}

#endif