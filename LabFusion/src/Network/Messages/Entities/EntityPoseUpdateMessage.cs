using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class EntityPoseUpdateData : INetSerializable
{
    public const int DefaultSize = sizeof(byte) + sizeof(ushort);
    public const int RigidbodySize = sizeof(float) * 9 + SerializedSmallQuaternion.Size;

    public ushort entityId;
    public EntityPose pose;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref entityId);
        serializer.SerializeValue(ref pose);
    }

    public NetworkEntity GetEntity()
    {
        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(entityId);
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
        if (entity == null || !entity.IsRegistered || entity.OwnerID == null || entity.OwnerID != received.Sender)
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