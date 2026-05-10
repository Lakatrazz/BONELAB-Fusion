using System.Numerics;

namespace LabFusion.Math.Numerics;

public static class NumericsDerivatives
{
    public static Vector3 GetAngularDisplacement(Quaternion from, Quaternion to)
    {
        var displacement = to * Quaternion.Inverse(from);
        displacement = Quaternion.Normalize(displacement).Shortest();

        displacement.ToAxisAngle(out var axis, out var angle);

        return axis * angle;
    }

    public static Quaternion GetQuaternionDisplacement(Vector3 angularDisplacement)
    {
        float length = angularDisplacement.Length();

        if (length < 0.001f)
        {
            return Quaternion.Identity;
        }

        Vector3 axis = angularDisplacement / length;

        return Quaternion.CreateFromAxisAngle(axis, length);
    }
}
