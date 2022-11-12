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

        // Credits to https://forum.unity.com/threads/encoding-vector2-and-vector3-variables-into-single-int-or-float-and-back.448346/
        public static ulong ToULong(this Vector3 vector3, bool safetyCheck = false, float precision = 1000f)
        {
            // Clamp the vector parameters if safety is on
            if (safetyCheck) {
                vector3 = Vector3.Max(Vector3.Min(vector3, Vector3.one * 320), Vector3.one * -320);
            }

            //Vectors must stay within the -320.00 to 320.00 range per axis - no error handling is coded here
            //Adds 32768 to get numbers into the 0-65536 range rather than -32768 to 32768 range to allow unsigned
            //Multiply by 100 to get two decimal place
            ulong xcomp = (ulong)(Mathf.RoundToInt((vector3.x * precision)) + 32768);
            ulong ycomp = (ulong)(Mathf.RoundToInt((vector3.y * precision)) + 32768);
            ulong zcomp = (ulong)(Mathf.RoundToInt((vector3.z * precision)) + 32768);

            return xcomp + ycomp * 65536 + zcomp * 4294967296;
        }

        // Credits to https://forum.unity.com/threads/encoding-vector2-and-vector3-variables-into-single-int-or-float-and-back.448346/
        public static Vector3 ToVector3(this ulong u, float precision = 1000f)
        {
            //Get the leftmost bits first. The fractional remains are the bits to the right.
            // 1024 is 2 ^ 10 - 1048576 is 2 ^ 20 - just saving some calculation time doing that in advance
            ulong z = (ulong)(u / 4294967296);
            ulong y = (ulong)((u - z * 4294967296) / 65536);
            ulong x = (ulong)(u - y * 65536 - z * 4294967296);

            // subtract 512 to move numbers back into the -512 to 512 range rather than 0 - 1024
            return new Vector3(((float)x - 32768f) / precision, ((float)y - 32768f) / precision, ((float)z - 32768f) / precision);
        }

    }
}
