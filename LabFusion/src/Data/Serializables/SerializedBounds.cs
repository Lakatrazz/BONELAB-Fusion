using LabFusion.Network.Serialization;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedBounds : INetSerializable
{
    public static SerializedBounds Fallback => new(Vector3.zero, Vector3.one * 0.1f);

    public Vector3 Center = Vector3.zero;

    public Vector3 Size = Vector3.zero;

    public SerializedBounds() { }

    public SerializedBounds(Vector3 center, Vector3 size)
    {
        Center = center;
        Size = size;
    }

    public SerializedBounds(Bounds bounds)
    {
        Center = bounds.center;
        Size = bounds.size;
    }

    public int? GetSize() => SerializedSmallVector3.Size * 2;

    public void Serialize(INetSerializer serializer)
    {
        SerializedSmallVector3 center = null;
        SerializedSmallVector3 size = null;

        if (!serializer.IsReader)
        {
            center = SerializedSmallVector3.Compress(this.Center);
            size = SerializedSmallVector3.Compress(this.Size);
        }

        serializer.SerializeValue(ref center);
        serializer.SerializeValue(ref size);

        if (serializer.IsReader)
        {
            this.Center = center.Expand();
            this.Size = size.Expand();
        }
    }

    public Bounds ToBounds() => new(Center, Size);
}
