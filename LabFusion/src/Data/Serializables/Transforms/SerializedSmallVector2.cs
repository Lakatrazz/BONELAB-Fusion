using LabFusion.Extensions;
using LabFusion.Network.Serialization;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedSmallVector2 : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(float);

    public int? GetSize() => Size;

    public sbyte X, Y;

    public float Magnitude;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref X);
        serializer.SerializeValue(ref Y);
        serializer.SerializeValue(ref Magnitude);
    }

    public static SerializedSmallVector2 Compress(Vector2 vector)
    {
        var normalized = vector.normalized;
        float magnitude = vector.magnitude;

        return new SerializedSmallVector2()
        {
            X = normalized.x.ToSByte(),
            Y = normalized.y.ToSByte(),
            Magnitude = magnitude,
        };
    }

    public Vector2 Expand()
    {
        var normalized = new Vector2(X.ToSingle(), Y.ToSingle()).normalized;
        return normalized * Magnitude;
    }
}