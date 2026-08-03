using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Senders;

namespace LabFusion.Network;

public enum PermissionCommandType
{
    UNKNOWN = 0,
    KICK = 1,
    BAN = 2,
    TELEPORT_TO_THEM = 3,
    TELEPORT_TO_ME = 4,
}

public class PermissionCommandRequestData : INetSerializable
{
    public PermissionCommandType Type;
    public byte? OtherPlayer;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
        serializer.SerializeValue(ref OtherPlayer);
    }
}

public class PermissionCommandRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PermissionCommandRequest;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PermissionCommandRequestData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        // Get the user
        var playerID = PlayerIDManager.GetPlayerID(sender.Value);

        // Get the user's permissions
        PlayerID otherPlayer = null;

        if (data.OtherPlayer.HasValue)
        {
            otherPlayer = PlayerIDManager.GetPlayerID(data.OtherPlayer.Value);
        }

        FusionPermissions.FetchPermissionLevel(playerID.PlatformID, out var level, out _);

        switch (data.Type)
        {
            case PermissionCommandType.UNKNOWN:
                break;
            case PermissionCommandType.KICK:
                if (otherPlayer.IsHost)
                {
                    return;
                }

                if (otherPlayer != null && FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.Kicking))
                {
                    NetworkHelper.KickUser(otherPlayer);
                }
                break;
            case PermissionCommandType.BAN:
                if (otherPlayer.IsHost)
                {
                    return;
                }

                if (otherPlayer != null && FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.Banning))
                {
                    NetworkHelper.BanUser(otherPlayer);
                }
                break;
            case PermissionCommandType.TELEPORT_TO_THEM:
                if (otherPlayer != null && FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.Teleportation))
                {
                    if (NetworkPlayerManager.TryGetPlayer(otherPlayer, out var otherNetPlayer) && otherNetPlayer.NetworkRig.HasRig)
                    {
                        PlayerSender.SendPlayerTeleport(playerID, otherNetPlayer.NetworkRig.RigRefs.RigManager.physicsRig.feet.transform.position);
                    }
                }
                break;
            case PermissionCommandType.TELEPORT_TO_ME:
                if (otherPlayer != null && FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.Teleportation))
                {
                    if (NetworkPlayerManager.TryGetPlayer(playerID, out var player) && player.NetworkRig.HasRig)
                    {
                        PlayerSender.SendPlayerTeleport(otherPlayer, player.NetworkRig.RigRefs.RigManager.physicsRig.feet.transform.position);
                    }
                }
                break;
        }
    }
}