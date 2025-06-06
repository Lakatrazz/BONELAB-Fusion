using LabFusion.Player;
using LabFusion.Entities;
using LabFusion.Utilities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PlayerPoseUpdateData : INetSerializable
{
    public const int Size = RigPose.Size;

    public int? GetSize() => Size;

    public RigPose Pose;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Pose);
    }

    public static PlayerPoseUpdateData Create(RigPose pose)
    {
        var data = new PlayerPoseUpdateData
        {
            Pose = pose,
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

        var playerId = received.Sender.Value;

        // Make sure this isn't us
        if (playerId == PlayerIDManager.LocalSmallID)
        {
            throw new Exception("Player received a pose for their own player.");
        }

        // Get network player
        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(playerId);

        if (entity == null)
        {
            FusionLogger.Error($"PlayerPoseUpdateMessage could not find an entity with id {playerId}!");
            return;
        }

        var networkPlayer = entity.GetExtender<NetworkPlayer>();

        if (networkPlayer == null)
        {
            FusionLogger.Error($"PlayerPoseUpdateMessage could not get a NetworkPlayer from the player entity!");
            return;
        }

        // Apply pose
        networkPlayer.OnReceivePose(data.Pose);
    }
}