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
    public byte smallId;
    public PermissionCommandType type;
    public byte? otherPlayer;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref type, Precision.OneByte);
        serializer.SerializeValue(ref otherPlayer);
    }

    public static PermissionCommandRequestData Create(byte smallId, PermissionCommandType type, byte? otherPlayer = null)
    {
        return new PermissionCommandRequestData()
        {
            smallId = smallId,
            type = type,
            otherPlayer = otherPlayer,
        };
    }
}

public class PermissionCommandRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PermissionCommandRequest;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PermissionCommandRequestData>();

        // Get the user
        PlayerID playerId = PlayerIDManager.GetPlayerID(data.smallId);

        // Check for spoofing
        if (NetworkInfo.IsSpoofed(playerId.PlatformID))
        {
            return;
        }

        // Get the user's permissions
        PlayerID otherPlayer = null;

        if (data.otherPlayer.HasValue)
        {
            otherPlayer = PlayerIDManager.GetPlayerID(data.otherPlayer.Value);
        }

        FusionPermissions.FetchPermissionLevel(playerId, out var level, out _);

        switch (data.type)
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
                    PlayerRepUtilities.TryGetReferences(otherPlayer, out var references);

                    if (references != null && references.IsValid)
                        PlayerSender.SendPlayerTeleport(playerId, references.RigManager.physicsRig.feet.transform.position);
                }
                break;
            case PermissionCommandType.TELEPORT_TO_ME:
                if (otherPlayer != null && FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.Teleportation))
                {
                    PlayerRepUtilities.TryGetReferences(playerId, out var references);

                    if (references != null && references.IsValid)
                        PlayerSender.SendPlayerTeleport(otherPlayer, references.RigManager.physicsRig.feet.transform.position);
                }
                break;
        }
    }
}