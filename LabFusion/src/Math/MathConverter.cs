using NumericsVector3 = System.Numerics.Vector3;
using NumericsQuaternion = System.Numerics.Quaternion;

using UnityVector3 = UnityEngine.Vector3;
using UnityQuaternion = UnityEngine.Quaternion;

namespace LabFusion.Math;

public static class MathConverter
{
    public static NumericsVector3 ToNumericsVector3(this UnityVector3 vector) => new(vector.x, vector.y, vector.z);
    public static NumericsQuaternion ToNumericsQuaternion(this UnityQuaternion quaternion) => new(quaternion.x, quaternion.y, quaternion.z, quaternion.w);

    public static UnityVector3 ToUnityVector3(this NumericsVector3 vector)
    {
        // Direct assignment appears to give a very slight edge in performance compared to interop's constructor
        UnityVector3 result;
        result.x = vector.X;
        result.y = vector.Y;
        result.z = vector.Z;
        return result;
    }
    public static UnityQuaternion ToUnityQuaternion(this NumericsQuaternion quaternion)
    {
        // Direct assignment appears to give a very slight edge in performance compared to interop's constructor
        UnityQuaternion result;
        result.x = quaternion.X;
        result.y = quaternion.Y;
        result.z = quaternion.Z;
        result.w = quaternion.W;
        return result;
    }
}
