using LabFusion.Extensions;
using LabFusion.Math;
using LabFusion.Math.Numerics;
using LabFusion.Utilities;

using UnityEngine;

using NumericsVector3 = System.Numerics.Vector3;

namespace LabFusion.Scene;

public static class NetworkTransformManager
{
    private static Vector3 _floatingOrigin = Vector3Extensions.Zero;
    public static Vector3 FloatingOrigin
    {
        get
        {
            return _floatingOrigin;
        }
        set
        {
            _floatingOrigin = value;
            _numericsFloatingOrigin = value.ToNumericsVector3();
        }
    }

    private static NumericsVector3 _numericsFloatingOrigin = NumericsVector3.Zero;
    public static NumericsVector3 NumericsFloatingOrigin => _numericsFloatingOrigin;

    public const float WorldLimit = 50000f;

    public const float WorldLimitSquared = WorldLimit * WorldLimit;

    public const float SpeedLimit = 1000f;

    public static NumericsVector3 EncodePosition(NumericsVector3 position) => position - NumericsFloatingOrigin;

    public static NumericsVector3 DecodePosition(NumericsVector3 position) => position + NumericsFloatingOrigin;

    public static Vector3 EncodePosition(Vector3 position) => position - FloatingOrigin;

    public static Vector3 DecodePosition(Vector3 position) => position + FloatingOrigin;

    public static Vector3 EncodeVelocity(Vector3 velocity) => velocity * TimeReferences.TimeScale;

    public static Vector3 DecodeVelocity(Vector3 velocity) => velocity / TimeReferences.SafeTimeScale;

    public static bool IsLinearDesynced(NumericsVector3 position, NumericsVector3 targetPosition, NumericsVector3 targetVelocity)
    {
        float error = (targetPosition - position).Length();

        float sqrtTargetVelocity = MathF.Sqrt(targetVelocity.Length());

        float maxDistance = 1f + sqrtTargetVelocity;

        if (error > maxDistance)
        {
            return true;
        }

        return false;
    }

    public static bool IsInBounds(NumericsVector3 position)
    {
        float lengthSquared = position.LengthSquared();

        if (float.IsNaN(lengthSquared)) 
        {
            return false;
        }

        if (lengthSquared >= WorldLimitSquared)
        {
            return false;
        }

        return true;
    }

    public static NumericsVector3 LimitVelocity(NumericsVector3 velocity)
    {
        return NumericsMathVector3.ClampMagnitude(velocity, SpeedLimit);
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
        if (sqrMagnitude >= WorldLimitSquared)
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
