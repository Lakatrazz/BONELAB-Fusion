using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SystemVector3 = System.Numerics.Vector3;
using SystemQuaternion = System.Numerics.Quaternion;
using Unity.Mathematics;

namespace LabFusion.Extensions
{
    public static class QuaternionExtensions
    {
        public static readonly Quaternion identity = Quaternion.identity;

        public const float kEpsilon = 0.000001F;

        public static float GetMagnitude(Quaternion quaternion)
        {
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
            Normalize(ref q, out Quaternion result);
            return result;
        }

        public static void Normalize(ref Quaternion q, out Quaternion result)
        {
            float scale = 1.0f / GetMagnitude(q);
            result = new Quaternion(q.x * scale, q.y * scale, q.z * scale, q.w * scale);
        }

        public static Quaternion AngleAxis(float angle, Vector3 axis)
        {
            if (axis.sqrMagnitude == 0.0f)
                return identity;

            Quaternion result = identity;
            var radians = angle * ManagedMathf.Deg2Rad;
            radians *= 0.5f;
            axis.Normalize();
            axis *= (float)Math.Sin(radians);
            result.x = axis.x;
            result.y = axis.y;
            result.z = axis.z;
            result.w = (float)Math.Cos(radians);

            return Normalize(result);
        }

        public static SystemQuaternion AngleAxis(float angle, SystemVector3 axis)
        {
            if (axis.LengthSquared() == 0.0f)
                return SystemQuaternion.Identity;

            SystemQuaternion result = SystemQuaternion.Identity;
            var radians = angle * ManagedMathf.Deg2Rad;
            radians *= 0.5f;
            axis = SystemVector3.Normalize(axis);
            axis *= (float)Math.Sin(radians);
            result.X = axis.X;
            result.Y = axis.Y;
            result.Z = axis.Z;
            result.W = (float)Math.Cos(radians);

            return SystemQuaternion.Normalize(result);
        }

        public static float Angle(SystemQuaternion a, SystemQuaternion b)
        {
            float dot = Math.Min(Math.Abs(SystemQuaternion.Dot(a, b)), 1.0F);
            return IsEqualUsingDot(dot) ? 0.0f : ManagedMathf.Acos(dot) * 2.0F * ManagedMathf.Rad2Deg;
        }

        private static bool IsEqualUsingDot(float dot)
        {
            return dot > 1.0f - kEpsilon;
        }

        public static void ToAngleAxis(this SystemQuaternion quaternion, out float angle, out SystemVector3 axis)
        {
            quaternion.ToAxisAngle(out axis, out angle);
            angle *= ManagedMathf.Rad2Deg;
        }

        public static void ToAxisAngle(this SystemQuaternion quaternion, out SystemVector3 axis, out float angle)
        {
            if (Math.Abs(quaternion.W) > 1.0f)
                quaternion = SystemQuaternion.Normalize(quaternion);
            angle = 2.0f * ManagedMathf.Acos(quaternion.W); // angle
            float den = (float)Math.Sqrt(1.0 - quaternion.W * quaternion.W);
            if (den > 0.0001f)
            {
                axis = new SystemVector3(quaternion.X, quaternion.Y, quaternion.Z) / den;
            }
            else
            {
                axis = new SystemVector3(1, 0, 0);
            }
        }

        public static SystemQuaternion ToSystemQuaternion(this Quaternion quaternion)
        {
            return new SystemQuaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public static Quaternion ToUnityQuaternion(this SystemQuaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }
    }
}
