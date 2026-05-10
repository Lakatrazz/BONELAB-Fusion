using NumericsVector3 = System.Numerics.Vector3;
using NumericsQuaternion = System.Numerics.Quaternion;

using UnityVector3 = UnityEngine.Vector3;
using UnityQuaternion = UnityEngine.Quaternion;

namespace LabFusion.Math;

public static class MathConverter
{
    public static NumericsVector3 ToNumericsVector3(this UnityVector3 vector) => new(vector.x, vector.y, vector.z);
    public static NumericsQuaternion ToNumericsQuaternion(this UnityQuaternion quaternion) => new(quaternion.x, quaternion.y, quaternion.z, quaternion.w);

    public static UnityVector3 ToUnityVector3(this NumericsVector3 vector) => new(vector.X, vector.Y, vector.Z);
    public static UnityQuaternion ToUnityQuaternion(this NumericsQuaternion quaternion) => new(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
}
