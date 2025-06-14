using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class ComponentIndexData : INetSerializable
{
    public const int Size = sizeof(ushort) * 2;

    public ushort EntityID;
    public ushort ComponentIndex;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EntityID);
        serializer.SerializeValue(ref ComponentIndex);
    }

    public static ComponentIndexData Create(ushort entityId, ushort componentIndex)
    {
        return new ComponentIndexData()
        {
            EntityID = entityId,
            ComponentIndex = componentIndex,
        };
    }
}