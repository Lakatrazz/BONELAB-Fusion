using LabFusion.Data;
using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Extensions {
    public static class Vector3Extensions {
        // Some Vector3 functions have a large performance intake in Melonloader for whatever reason
        public static readonly Vector3 zero = Vector3.zero;
        public static readonly Vector3 one = Vector3.one;

        public static readonly Vector3 left = Vector3.left;
        public static readonly Vector3 right = Vector3.right;
        public static readonly Vector3 forward = Vector3.forward;
        public static readonly Vector3 up = Vector3.up;

        public static bool IsNanOrInf(this Vector3 vector) {
            return Internal_IsNanOrInf(vector.x) || Internal_IsNanOrInf(vector.y) || Internal_IsNanOrInf(vector.z);
        }

        internal static bool Internal_IsNanOrInf(float value) {
            return float.IsPositiveInfinity(value) || float.IsNegativeInfinity(value) || float.IsNaN(value);
        }

        public static Quaternion GetQuaternionDisplacement(this Vector3 displacement)
        {
            float xMag = GetMagnitude(displacement) * MathExtensions.Rad2Deg;
            Vector3 x = Normalize(displacement);
            
            return QuaternionExtensions.AngleAxis(xMag, x);
        }

        public static Vector3 Normalize(Vector3 vector3) {
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
    }
}
