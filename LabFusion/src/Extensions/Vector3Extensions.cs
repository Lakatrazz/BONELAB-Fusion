using LabFusion.Math;

using UnityEngine;

namespace LabFusion.Extensions;

using System;

public static class Vector3Extensions
{
    // Due to the nature of IL2, getting these values is a little slower than usual
    // So, doesn't hurt to cache them
    public static readonly Vector3 Zero = Vector3.zero;
    public static readonly Vector3 One = Vector3.one;

    public static readonly Vector3 Left = Vector3.left;
    public static readonly Vector3 Right = Vector3.right;

    public static readonly Vector3 Forward = Vector3.forward;
    public static readonly Vector3 Back = Vector3.back;

    public static readonly Vector3 Up = Vector3.up;
    public static readonly Vector3 Down = Vector3.down;

    public static Quaternion GetQuaternionDisplacement(this Vector3 displacement)
    {
        float xMag = GetMagnitude(displacement) * ManagedMathf.Rad2Deg;
        Vector3 x = Normalize(displacement);

        return Quaternion.AngleAxis(xMag, x);
    }

    public static Vector3 Normalize(Vector3 vector3)
    {
        return vector3 / GetMagnitude(vector3);
    }

    public static float GetMagnitude(Vector3 vector3)
    {
        float x = vector3.x;
        float y = vector3.y;
        float z = vector3.z;
        return (float)Math.Sqrt(x * x + y * y + z * z);
    }

    public static float GetSqrMagnitude(Vector3 vector3)
    {
        float x = vector3.x;
        float y = vector3.y;
        float z = vector3.z;
        return x * x + y * y + z * z;
    }

    /// <summary>
    /// Returns if any of the components of this vector are NaN.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static bool IsNaN(this Vector3 vector)
    {
        // NaN added to any value will always return NaN
        return float.IsNaN(vector.x + vector.y + vector.z);
    }
}