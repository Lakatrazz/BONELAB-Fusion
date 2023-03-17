using Il2CppSystem.Runtime.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Extensions {
    public static class QuaternionExtensions {
        public static readonly Quaternion identity = Quaternion.identity;

        public static float GetMagnitude(Quaternion quaternion) {
            float x = quaternion.x;
            float y = quaternion.y;
            float z = quaternion.z;
            float w = quaternion.w;
            return (float)Math.Sqrt(x * x + y * y + z * z + w * w);
        }

        public static float GetSqrMagnitude(Quaternion quaternion)
        {
            float x = quaternion.x;
            float y = quaternion.y;
            float z = quaternion.z;
            float w = quaternion.w;
            return x * x + y * y + z * z + w * w;
        }

        public static Quaternion Normalize(Quaternion q)
        {
            Quaternion result;
            Normalize(ref q, out result);
            return result;
        }

        public static void Normalize(ref Quaternion q, out Quaternion result)
        {
            float scale = 1.0f / GetMagnitude(q);
            result = new Quaternion(q.x * scale, q.y * scale, q.z * scale, q.w * scale);
        }

        public static Quaternion AngleAxis(float angle, Vector3 axis) {
            if (axis.sqrMagnitude == 0.0f)
                return identity;

            Quaternion result = identity;
            var radians = angle * MathExtensions.Deg2Rad;
            radians *= 0.5f;
            axis.Normalize();
            axis = axis * (float)Math.Sin(radians);
            result.x = axis.x;
            result.y = axis.y;
            result.z = axis.z;
            result.w = (float)Math.Cos(radians);

            return Normalize(result);
        }
    }
}
