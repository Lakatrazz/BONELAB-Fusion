using LabFusion.Extensions;
using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedSmallDirection2D : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2;

    public sbyte x, y;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(x);
        writer.Write(y);
    }

    public void Deserialize(FusionReader reader)
    {
        x = reader.ReadSByte();
        y = reader.ReadSByte();
    }

    public static SerializedSmallDirection2D Compress(Vector2 direction)
    {
        var normalized = direction.normalized;

        return new SerializedSmallDirection2D()
        {
            x = normalized.x.ToSByte(),
            y = normalized.y.ToSByte(),
        };
    }

    public Vector2 Expand()
    {
        var normalized = new Vector2(x.ToSingle(), y.ToSingle());
        return normalized;
    }
}