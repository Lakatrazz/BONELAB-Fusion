using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class ComponentIndexData : INetSerializable
{
    public const int Size = sizeof(ushort) * 2;

    public ushort EntityId;
    public ushort ComponentIndex;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EntityId);
        serializer.SerializeValue(ref ComponentIndex);
    }

    public static ComponentIndexData Create(ushort entityId, ushort componentIndex)
    {
        return new ComponentIndexData()
        {
            EntityId = entityId,
            ComponentIndex = componentIndex,
        };
    }
}