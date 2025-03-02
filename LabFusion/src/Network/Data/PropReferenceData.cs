using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PropReferenceData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte smallId;
    public ushort syncId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref syncId);
    }

    public static PropReferenceData Create(byte smallId, ushort syncId)
    {
        return new PropReferenceData()
        {
            smallId = smallId,
            syncId = syncId,
        };
    }
}