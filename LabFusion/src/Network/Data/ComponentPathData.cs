using LabFusion.Data;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class ComponentPathData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort) * 2 + ComponentHashData.Size;

    public bool HasEntity;

    public ushort EntityId;
    public ushort ComponentIndex;

    public ComponentHashData HashData;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref HasEntity);
        serializer.SerializeValue(ref EntityId);
        serializer.SerializeValue(ref ComponentIndex);
        serializer.SerializeValue(ref HashData);
    }

    public static ComponentPathData Create(bool hasEntity, ushort entityId, ushort componentIndex, ComponentHashData hashData)
    {
        return new ComponentPathData()
        {
            HasEntity = hasEntity,
            EntityId = entityId,
            ComponentIndex = componentIndex,
            HashData = hashData,
        };
    }
}
