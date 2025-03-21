using UnityEngine;

namespace LabFusion.Network.Serialization;

public static class NetSerializerUnityExtensions
{
    public static void SerializeValue(this INetSerializer serializer, ref Color color)
    {
        serializer.SerializeValue(ref color.r);
        serializer.SerializeValue(ref color.g);
        serializer.SerializeValue(ref color.b);
        serializer.SerializeValue(ref color.a);
    }

    public static void SerializeValue(this INetSerializer serializer, ref Quaternion value)
    {
        serializer.SerializeValue(ref value.x);
        serializer.SerializeValue(ref value.y);
        serializer.SerializeValue(ref value.z);
        serializer.SerializeValue(ref value.w);
    }

    public static void SerializeValue(this INetSerializer serializer, ref Vector4 value)
    {
        serializer.SerializeValue(ref value.x);
        serializer.SerializeValue(ref value.y);
        serializer.SerializeValue(ref value.z);
        serializer.SerializeValue(ref value.w);
    }

    public static void SerializeValue(this INetSerializer serializer, ref Vector3 value)
    {
        serializer.SerializeValue(ref value.x);
        serializer.SerializeValue(ref value.y);
        serializer.SerializeValue(ref value.z);
    }

    public static void SerializeValue(this INetSerializer serializer, ref Vector2 value)
    {
        serializer.SerializeValue(ref value.x);
        serializer.SerializeValue(ref value.y);
    }
}
