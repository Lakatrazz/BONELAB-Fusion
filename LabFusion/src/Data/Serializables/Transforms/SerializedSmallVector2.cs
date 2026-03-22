using LabFusion.Extensions;
using LabFusion.Network.Serialization;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedSmallVector2 : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(float);

    public int? GetSize() => Size;

    public sbyte x, y;
    public float magnitude;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref y);
        serializer.SerializeValue(ref magnitude);
    }

    public static SerializedSmallVector2 Compress(Vector2 vector)
    {
        var normalized = vector.normalized;
        float magnitude = vector.magnitude;

        return new SerializedSmallVector2()
        {
            x = normalized.x.ToSByte(),
            y = normalized.y.ToSByte(),
            magnitude = magnitude,
        };
    }

    public Vector2 Expand()
    {
        var normalized = new Vector2(x.ToSingle(), y.ToSingle()).normalized;
        return normalized * magnitude;
    }
}