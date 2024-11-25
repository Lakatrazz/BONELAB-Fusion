using LabFusion.Data;

namespace LabFusion.Network;

public class ComponentPathData : IFusionSerializable
{
    public bool hasNetworkEntity;

    public ushort entityId;
    public ushort componentIndex;

    public ComponentHashData hashData;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(hasNetworkEntity);

        writer.Write(entityId);
        writer.Write(componentIndex);

        writer.Write(hashData);
    }

    public void Deserialize(FusionReader reader)
    {
        hasNetworkEntity = reader.ReadBoolean();

        entityId = reader.ReadUInt16();
        componentIndex = reader.ReadUInt16();

        hashData = reader.ReadFusionSerializable<ComponentHashData>();
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
