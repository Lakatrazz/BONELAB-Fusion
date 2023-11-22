#ifndef POSESPACE_INCLUDED
#define POSESPACE_INCLUDED

#define HitArrayCount 32

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"

//Unity REQUIRES you to use the UnityPerMaterial cbuffer for batching. Seems like this could be done better.
// CBUFFER_START(hitBuffer)
// float4x4 EllipsoidPosArray[HitArrayCount];
// int _NumberOfHits;
// CBUFFER_END

TEXTURE2D(_HitRamp); SAMPLER(sampler_HitRamp);

inline half2 GetClosestImpactUV( float3 Posespace, float4x4 EllipsoidPosArray[HitArrayCount], int NumberOfHits )
{
    half HitDistance = 1;
    half3 closestHit = half3(0,0,0);
    for ( int i = 0; i <NumberOfHits; i++ ){
        half3 LocalPosP = half3(Posespace - float3(EllipsoidPosArray[i][0][3],EllipsoidPosArray[i][1][3],EllipsoidPosArray[i][2][3]));
        half3 localspace = mul( LocalPosP , (half3x3)EllipsoidPosArray[i] ).xyz;
        half3 currentdist = saturate(  length( localspace));
        closestHit = currentdist < HitDistance ? localspace : closestHit;
        HitDistance =  min( HitDistance, currentdist );
    }
    half HitRadial = atan2(closestHit.x, closestHit.y) * INV_PI;
    return float2(HitDistance,HitRadial);
}


inline half4 SampleHitTexture(half2 ImpactsUV){
    return _HitRamp.SampleLevel(sampler_HitRamp,ImpactsUV,0);
}

#endif