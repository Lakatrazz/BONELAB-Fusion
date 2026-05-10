using UnityEngine;

namespace LabFusion.Math.Unity;

public static class UnityMathQuaternion
{
    public static Quaternion Shortest(this Quaternion quaternion)
    {
        if (quaternion.w < 0f)
        {
            quaternion.x = -quaternion.x;
            quaternion.y = -quaternion.y;
            quaternion.z = -quaternion.z;
            quaternion.w = -quaternion.w;
        }

        return quaternion;
    }
}