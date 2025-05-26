using LabFusion.Player;
using LabFusion.Entities;
using LabFusion.Utilities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PlayerPoseUpdateData : INetSerializable
{
    public const int Size = RigPose.Size;

    public RigPose pose;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref pose);
    }

    public static PlayerPoseUpdateData Create(RigPose pose)
    {
        var data = new PlayerPoseUpdateData
        {
            pose = pose,
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
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(playerId);

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
        networkPlayer.OnReceivePose(data.pose);
    }
}