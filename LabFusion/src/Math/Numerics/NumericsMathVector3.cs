using System.Numerics;

namespace LabFusion.Math.Numerics;

public static class NumericsMathVector3
{
    public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
    {
        float length = vector.Length();

        if (length > maxLength)
        {
            return vector * (maxLength / length);
        }

        return vector;
    }
}
