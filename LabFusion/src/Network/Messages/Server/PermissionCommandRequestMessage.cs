using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Preferences.Server;
using LabFusion.Senders;
using LabFusion.Exceptions;

namespace LabFusion.Network;

public enum PermissionCommandType
{
    UNKNOWN = 0,
    KICK = 1,
    BAN = 2,
    TELEPORT_TO_THEM = 3,
    TELEPORT_TO_US = 4,
}

public class PermissionCommandRequestData : IFusionSerializable
{
    public byte smallId;
    public PermissionCommandType type;
    public byte? otherPlayer;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write((byte)type);
        writer.Write(otherPlayer);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        type = (PermissionCommandType)reader.ReadByte();
        otherPlayer = reader.ReadByteNullable();
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

public class PermissionCommandRequestMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.PermissionCommandRequest;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // This should only ever be handled by the server
        if (!isServerHandled)
        {
            throw new ExpectedServerException();
        }

        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<PermissionCommandRequestData>();

        // Get the user
        PlayerId playerId = PlayerIdManager.GetPlayerId(data.smallId);

        // Check for spoofing
        if (NetworkInfo.IsSpoofed(playerId.LongId))
        {
            return;
        }

        // Get the user's permissions
        PlayerId otherPlayer = null;

        if (data.otherPlayer.HasValue)
        {
            otherPlayer = PlayerIdManager.GetPlayerId(data.otherPlayer.Value);
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
            case PermissionCommandType.TELEPORT_TO_US:
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