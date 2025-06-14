using LabFusion.Extensions;

using UnityEngine;

namespace LabFusion.Scene;

public static class NetworkTransformManager
{
    public static Vector3 FloatingOrigin { get; set; } = Vector3Extensions.zero;

    public const float WorldLimit = 50000f;

    public const float WorldLimitSquared = WorldLimit * WorldLimit;

    public const float SpeedLimit = 1000f;

    public static Vector3 EncodePosition(Vector3 position)
    {
        return position - FloatingOrigin;
    }

    public static Vector3 DecodePosition(Vector3 position) 
    { 
        return position + FloatingOrigin; 
    }

    public static bool IsInBounds(Vector3 position)
    {
        float sqrMagnitude = position.sqrMagnitude;

        // Make sure the vector isn't NaN
        if (float.IsNaN(sqrMagnitude))
        {
            return false;
        }

        // Check limits
        if (position.sqrMagnitude >= WorldLimitSquared)
        {
            return false;
        }

        // Passed all checks
        return true;
    }

    public static Vector3 LimitVelocity(Vector3 velocity)
    {
        return Vector3.ClampMagnitude(velocity, SpeedLimit);
    }
}
