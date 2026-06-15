using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Marrow.Serialization;

public sealed class SerializedSpawnData : INetSerializable
{
    public int? GetSize() => Barcode.GetSize() + SerializedTransform.Size + sizeof(uint) + sizeof(bool) + sizeof(byte) + Bounds.GetSize();

    public string Barcode;

    public SerializedTransform SerializedTransform;

    public uint TrackerID;

    public bool SpawnEffect;

    public EntitySource SpawnSource;

    public SerializedBounds Bounds = SerializedBounds.Fallback;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Barcode);
        serializer.SerializeValue(ref SerializedTransform);
        serializer.SerializeValue(ref TrackerID);
        serializer.SerializeValue(ref SpawnEffect);
        serializer.SerializeValue(ref SpawnSource, Precision.OneByte);
        serializer.SerializeValue(ref Bounds);
    }
}
