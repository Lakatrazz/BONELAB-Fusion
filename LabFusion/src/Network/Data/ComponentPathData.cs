using LabFusion.Data;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class ComponentPathData : INetSerializable
{
    public bool hasNetworkEntity;

    public ushort entityId;
    public ushort componentIndex;

    public ComponentHashData hashData;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref hasNetworkEntity);
        serializer.SerializeValue(ref entityId);
        serializer.SerializeValue(ref componentIndex);
        serializer.SerializeValue(ref hashData);
    }

    public static ComponentPathData Create(bool hasNetworkEntity, ushort entityId, ushort componentIndex, ComponentHashData hashData)
    {
        return new ComponentPathData()
        {
            hasNetworkEntity = hasNetworkEntity,
            entityId = entityId,
            componentIndex = componentIndex,
            hashData = hashData,
        };
    }
}
