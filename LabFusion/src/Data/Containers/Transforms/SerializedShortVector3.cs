using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedShortVector3 : IFusionSerializable
{
    public const int Size = sizeof(short) * 3 + sizeof(float);

    public short x, y, z;
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
        x = reader.ReadInt16();
        y = reader.ReadInt16();
        z = reader.ReadInt16();
        magnitude = reader.ReadSingle();
    }

    public static SerializedShortVector3 Compress(Vector3 vector)
    {
        var normalized = vector.normalized;
        float magnitude = vector.magnitude;

        return new SerializedShortVector3()
        {
            x = ToShort(normalized.x),
            y = ToShort(normalized.y),
            z = ToShort(normalized.z),
            magnitude = magnitude,
        };
    }

    public Vector3 Expand()
    {
        var normalized = new Vector3(ToSingle(x), ToSingle(y), ToSingle(z)).normalized;
        return normalized * magnitude;
    }

    private static short ToShort(float value)
    {
        return (short)(value * 30000f);
    }

    private static float ToSingle(short value)
    {
        return (float)(value) / 30000f;
    }
}
