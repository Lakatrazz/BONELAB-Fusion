using System.Numerics;

namespace LabFusion.Extensions;

public static class NumericsVector3Extensions
{
    /// <summary>
    /// Returns if any of the components of this vector are NaN.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static bool IsNaN(this Vector3 vector)
    {
        // NaN added to any value will always return NaN
        return float.IsNaN(vector.X + vector.Y + vector.Z);
    }
}
