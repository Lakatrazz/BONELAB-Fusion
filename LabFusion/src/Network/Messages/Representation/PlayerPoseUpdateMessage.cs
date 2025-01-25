using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Entities;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class PlayerPoseUpdateData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(float) * 7 + SerializedLocalTransform.Size
        * RigAbstractor.TransformSyncCount + SerializedTransform.Size + SerializedSmallQuaternion.Size + SerializedController.Size * 2;

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
        var health = RigData.Refs.Health;

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
public class PlayerPoseUpdateMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerPoseUpdate;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerPoseUpdateData>();

        // Make sure this isn't us
        if (data.playerId == PlayerIdManager.LocalSmallId)
        {
            throw new Exception("Player received a pose for their own player.");
        }

        // Get network player
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.playerId);

        if (entity == null)
        {
            FusionLogger.Error($"PlayerPoseUpdateMessage could not find an entity with id {data.playerId}!");
            return;
        }

        var networkPlayer = entity.GetExtender<NetworkPlayer>();

        if (networkPlayer == null)
        {
            FusionLogger.Error($"PlayerPoseUpdateMessage could not get a NetworkPlayer from the player entity!");
            return;
        }

        // Apply pose
        networkPlayer.OnReceivePose(data.pose);
    }
}