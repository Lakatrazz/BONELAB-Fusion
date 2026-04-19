using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class ComponentIndexData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + sizeof(ushort);

    public NetworkEntityReference Entity;
    public ushort ComponentIndex;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Entity);
        serializer.SerializeValue(ref ComponentIndex);
    }

    public static ComponentIndexData Create(ushort entityID, ushort componentIndex)
    {
        return new ComponentIndexData()
        {
            Entity = new(entityID),
            ComponentIndex = componentIndex,
        };
    }
}