using LabFusion.Extensions;
using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedSmallVector3 : IFusionSerializable
{
    public const int Size = sizeof(byte) * 3 + sizeof(float);

    public sbyte x, y, z;
    public float magnitude;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(x);
        writer.Write(y);
        writer.Write(z);
        writer.Write(magnitude);
    }

    public void Deserialize(FusionReader reader)
    {
        x = reader.ReadSByte();
        y = reader.ReadSByte();
        z = reader.ReadSByte();
        magnitude = reader.ReadSingle();
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
