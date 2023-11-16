using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities
{
    // Taken from https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Math/Mathf.cs
    internal static class ManagedMathf
    {
        private struct MathfInternal
        {
            public static volatile float FloatMinNormal = 1.17549435E-38f;
            public static volatile float FloatMinDenormal = float.Epsilon;

            public static bool IsFlushToZeroEnabled = (FloatMinDenormal == 0);
        }

        public static readonly float Epsilon =
    MathfInternal.IsFlushToZeroEnabled ? MathfInternal.FloatMinNormal
    : MathfInternal.FloatMinDenormal;

        // Rad2Deg is 360 / (PI * 2) Aka 180 / PI
        public const float Rad2Deg = 57.2957795131f;

        // Deg2Rad is inverse Rad2Deg
        public const float Deg2Rad = 1f / Rad2Deg;

        public static float Sin(float f)
        {
            return (float)Math.Sin(f);
        }

        public static float Acos(float f)
        {
            return (float)Math.Acos(f);
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        public static float Clamp01(float value)
        {
            if (value < 0f)
                return 0f;
            else if (value > 1f)
                return 1f;
            else
                return value;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static int FloorToInt(float f) { return (int)Math.Floor(f); }

        public static int CeilToInt(float f) { return (int)Math.Ceiling(f); }

        public static bool Approximately(float a, float b)
        {
            return Math.Abs(b - a) < Math.Max(0.000001f * Math.Max(Math.Abs(a), Math.Abs(b)), Epsilon * 8);
        }
    }
}
