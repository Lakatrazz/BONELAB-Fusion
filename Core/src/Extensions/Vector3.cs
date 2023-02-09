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
        // Rad2Deg is 360 / (PI * 2) Aka 180 / PI
        public const float Rad2Deg = 57.2957795131f;

        public static bool IsNanOrInf(this Vector3 vector) {
            return Internal_IsNanOrInf(vector.x) || Internal_IsNanOrInf(vector.y) || Internal_IsNanOrInf(vector.z);
        }

        internal static bool Internal_IsNanOrInf(float value) {
            return float.IsPositiveInfinity(value) || float.IsNegativeInfinity(value) || float.IsNaN(value);
        }

        public static Quaternion GetQuaternionDisplacement(this Vector3 displacement)
        {
            float xMag = displacement.magnitude * Rad2Deg;
            Vector3 x = displacement.normalized;

            return Quaternion.AngleAxis(xMag, x);
        }
    }
}
