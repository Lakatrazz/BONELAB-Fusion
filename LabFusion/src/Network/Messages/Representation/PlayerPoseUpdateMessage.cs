using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Entities;

namespace LabFusion.Network;

public class PlayerPoseUpdateData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(float) * 7 + SerializedLocalTransform.Size
        * RigAbstractor.TransformSyncCount + SerializedTransform.Size + SerializedSmallQuaternion.Size + SerializedHand.Size * 2;

    public byte playerId;

    public float health;

    public RigPose pose;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(playerId);

        writer.Write(pose);

        writer.Write(health);
    }

    public void Deserialize(FusionReader reader)
    {
        playerId = reader.ReadByte();

        pose = reader.ReadFusionSerializable<RigPose>();

        health = reader.ReadSingle();
    }

    public static PlayerPoseUpdateData Create(byte playerId, RigPose pose)
    {
        var health = RigData.RigReferences.Health;

        var data = new PlayerPoseUpdateData
        {
            playerId = playerId,
            pose = pose,
            health = health.curr_Health,
        };

        return data;
    }
}

[Net.SkipHandleWhileLoading]
public class PlayerPoseUpdateMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.PlayerPoseUpdate;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<PlayerPoseUpdateData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.playerId, NetworkChannel.Unreliable, message, false);
            return;
        }

        // Make sure this isn't us
        if (data.playerId == PlayerIdManager.LocalSmallId)
        {
            throw new Exception("Player received a pose for their own player.");
        }

        // Get network player
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.playerId);

        if (entity == null)
        {
            return;
        }

        var networkPlayer = entity.GetExtender<NetworkPlayer>();

        if (networkPlayer == null)
        {
            return;
        }

        // Apply pose
        networkPlayer.OnReceivePose(data.pose);
    }
}