using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Extensions {
    public static class Vector3Extensions {
        public static bool IsNanOrInf(this Vector3 vector) {
            return Internal_IsNanOrInf(vector.x) || Internal_IsNanOrInf(vector.y) || Internal_IsNanOrInf(vector.z);
        }

        internal static bool Internal_IsNanOrInf(float value) {
            return float.IsPositiveInfinity(value) || float.IsNegativeInfinity(value) || float.IsNaN(value);
        }
    }
}
