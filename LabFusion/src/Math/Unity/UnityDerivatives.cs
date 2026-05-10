using UnityEngine;

namespace LabFusion.Math.Unity;

public static class UnityDerivatives
{
    public static Vector3 GetAngularDisplacement(Quaternion from, Quaternion to)
    {
        var displacement = to * Quaternion.Inverse(from);
        displacement = displacement.Shortest();

        displacement.ToAngleAxis(out var angle, out var axis);

        float magnitude = angle * ManagedMathf.Deg2Rad;

        return axis * magnitude;
    }

    public static Quaternion GetQuaternionDisplacement(Vector3 angularDisplacement)
    {
        float magnitude = angularDisplacement.magnitude;
        float angle = magnitude * ManagedMathf.Rad2Deg;

        Vector3 axis = angularDisplacement / magnitude;

        return Quaternion.AngleAxis(angle, axis);
    }
}