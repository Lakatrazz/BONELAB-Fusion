using LabFusion.Extensions;
using LabFusion.Network.Serialization;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedSmallVector3 : INetSerializable
{
    public const int Size = sizeof(byte) * 3 + sizeof(float);

    public sbyte x, y, z;
    public float magnitude;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref y);
        serializer.SerializeValue(ref z);
        serializer.SerializeValue(ref magnitude);
    }

    public static SerializedSmallVector3 Compress(Vector3 vector)
    {
        var normalized = vector.normalized;
        float magnitude = vector.magnitude;

        return new SerializedSmallVector3()
        {
            x = normalized.x.ToSByte(),
            y = normalized.y.ToSByte(),
            z = normalized.z.ToSByte(),
            magnitude = magnitude,
        };
    }

    public Vector3 Expand()
    {
        var normalized = new Vector3(x.ToSingle(), y.ToSingle(), z.ToSingle()).normalized;
        return normalized * magnitude;
    }
}
