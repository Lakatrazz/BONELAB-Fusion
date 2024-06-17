using LabFusion.Network;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.AI;

namespace LabFusion.Data;

public class SerializedTriggerRefReference : IFusionSerializable
{
    private enum ReferenceType
    {
        UNKNOWN = 0,
        NETWORK_ENTITY = 1,
        NULL = 2,
    }

    // Base trigger ref reference
    public TriggerRefProxy proxy;

    public SerializedTriggerRefReference() { }

    public SerializedTriggerRefReference(TriggerRefProxy proxy)
    {
        this.proxy = proxy;
    }

    public void Serialize(FusionWriter writer)
    {
        if (proxy == null)
        {
            writer.Write((byte)ReferenceType.NULL);
            return;
        }

        // Check if there is an entity, and write it
        if (TriggerRefProxyExtender.Cache.TryGet(proxy, out var entity))
        {
            writer.Write((byte)ReferenceType.NETWORK_ENTITY);
            writer.Write(entity.Id);
            return;
        }

        // No entity? Just write unknown
        writer.Write((byte)ReferenceType.UNKNOWN);
    }

    public void Deserialize(FusionReader reader)
    {
        var type = (ReferenceType)reader.ReadByte();

        switch (type)
        {
            default:
            case ReferenceType.UNKNOWN:
            case ReferenceType.NULL:
                // Do nothing for null
                break;
            case ReferenceType.NETWORK_ENTITY:
                var id = reader.ReadUInt16();

                var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(id);

                if (entity == null)
                {
                    break;
                }

                var extender = entity.GetExtender<TriggerRefProxyExtender>();

                if (extender == null)
                {
                    break;
                }

                proxy = extender.Component;
                break;
        }
    }
}