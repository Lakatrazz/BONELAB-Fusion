using LabFusion.Data;
using LabFusion.Entities;

using UnityEngine;

namespace LabFusion.Network;

public class EntityPoseUpdateData : IFusionSerializable
{
    public const int DefaultSize = sizeof(byte) + sizeof(ushort);
    public const int RigidbodySize = sizeof(float) * 9 + SerializedSmallQuaternion.Size;

    public byte ownerId;
    public ushort entityId;
    public EntityPose pose;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(ownerId);
        writer.Write(entityId);
        writer.Write(pose);
    }

    public void Deserialize(FusionReader reader)
    {
        ownerId = reader.ReadByte();
        entityId = reader.ReadUInt16();
        pose = reader.ReadFusionSerializable<EntityPose>();
    }

    public NetworkEntity GetEntity()
    {
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(entityId);
        return entity;
    }

    public static EntityPoseUpdateData Create(byte ownerId, ushort entityId, EntityPose pose)
    {
        var data = new EntityPoseUpdateData
        {
            ownerId = ownerId,
            entityId = entityId,
            pose = pose,
        };

        return data;
    }
}

[Net.SkipHandleWhileLoading]
public class EntityPoseUpdateMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.EntityPoseUpdate;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<EntityPoseUpdateData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.ownerId, NetworkChannel.Unreliable, message);
        }

        // Find the network entity
        var entity = data.GetEntity();

        // Validate the entity
        if (entity == null || !entity.IsRegistered || entity.OwnerId == null || entity.OwnerId != data.ownerId)
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