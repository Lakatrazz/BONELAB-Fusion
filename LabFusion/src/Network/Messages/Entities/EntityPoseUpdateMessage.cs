using LabFusion.Data;
using LabFusion.Entities;

namespace LabFusion.Network;

public class EntityPoseUpdateData : IFusionSerializable
{
    public const int DefaultSize = sizeof(byte) + sizeof(ushort);
    public const int RigidbodySize = sizeof(float) * 9 + SerializedSmallQuaternion.Size;

    public ushort entityId;
    public EntityPose pose;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(entityId);
        writer.Write(pose);
    }

    public void Deserialize(FusionReader reader)
    {
        entityId = reader.ReadUInt16();
        pose = reader.ReadFusionSerializable<EntityPose>();
    }

    public NetworkEntity GetEntity()
    {
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(entityId);
        return entity;
    }

    public static EntityPoseUpdateData Create(ushort entityId, EntityPose pose)
    {
        var data = new EntityPoseUpdateData
        {
            entityId = entityId,
            pose = pose,
        };

        return data;
    }
}

[Net.SkipHandleWhileLoading]
public class EntityPoseUpdateMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityPoseUpdate;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<EntityPoseUpdateData>();

        // Find the network entity
        var entity = data.GetEntity();

        // Validate the entity
        if (entity == null || !entity.IsRegistered || entity.OwnerId == null || entity.OwnerId != received.Sender)
        {
            return;
        }

        // Get the network prop so we can update its pose
        var networkProp = entity.GetExtender<NetworkProp>();

        if (networkProp == null)
        {
            return;
        }

        networkProp.OnReceivePose(data.pose);
    }
}