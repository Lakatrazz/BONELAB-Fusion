using System.Numerics;

namespace LabFusion.Math.Numerics;

public static class NumericsMathQuaternion
{
    public static Quaternion Shortest(this Quaternion quaternion)
    {
        if (quaternion.W < 0f)
        {
            quaternion.X = -quaternion.X;
            quaternion.Y = -quaternion.Y;
            quaternion.Z = -quaternion.Z;
            quaternion.W = -quaternion.W;
        }

        return quaternion;
    }

    public static void ToAxisAngle(this Quaternion quaternion, out Vector3 axis, out float angle)
    {
        quaternion = Quaternion.Normalize(quaternion);

        angle = 2.0f * MathF.Acos(quaternion.W);

        float sinHalfAngle = MathF.Sqrt(1.0f - quaternion.W * quaternion.W);

        if (sinHalfAngle < 0.001f)
        {
            axis = Vector3.UnitX;
        }
        else
        {
            axis = new Vector3(quaternion.X, quaternion.Y, quaternion.Z) / sinHalfAngle;
        }
    }
}
