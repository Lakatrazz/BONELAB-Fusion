using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class ComponentIndexData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte smallId;
    public ushort syncId;
    public byte componentIndex;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref syncId);
        serializer.SerializeValue(ref componentIndex);
    }

    public static ComponentIndexData Create(byte smallId, ushort syncId, byte componentIndex)
    {
        return new ComponentIndexData()
        {
            smallId = smallId,
            syncId = syncId,
            componentIndex = componentIndex,
        };
    }
}