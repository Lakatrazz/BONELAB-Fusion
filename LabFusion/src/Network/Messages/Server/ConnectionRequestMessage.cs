using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Preferences;
using LabFusion.Preferences.Server;
using LabFusion.Senders;
using LabFusion.Exceptions;
using LabFusion.Entities;

namespace LabFusion.Network;

public class ConnectionRequestData : IFusionSerializable
{
    public ulong longId;
    public Version version;
    public string avatarBarcode;
    public SerializedAvatarStats avatarStats;
    public FusionDictionary<string, string> initialMetadata;
    public List<string> initialEquippedItems;

    public bool IsValid { get; private set; } = true;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(longId);
        writer.Write(version);
        writer.Write(avatarBarcode);
        writer.Write(avatarStats);
        writer.Write(initialMetadata);
        writer.Write(initialEquippedItems);
    }

    public void Deserialize(FusionReader reader)
    {
        try
        {
            longId = reader.ReadUInt64();
            version = reader.ReadVersion();
            avatarBarcode = reader.ReadString();
            avatarStats = reader.ReadFusionSerializable<SerializedAvatarStats>();
            initialMetadata = reader.ReadStringDictionary();
            initialEquippedItems = reader.ReadStrings().ToList();
        }
        catch
        {
            IsValid = false;
        }
    }

    public static ConnectionRequestData Create(ulong longId, Version version, string avatarBarcode, SerializedAvatarStats stats)
    {
        return new ConnectionRequestData()
        {
            longId = longId,
            version = version,
            avatarBarcode = avatarBarcode,
            avatarStats = stats,
            initialMetadata = InternalServerHelpers.GetInitialMetadata(),
            initialEquippedItems = InternalServerHelpers.GetInitialEquippedItems(),
        };
    }
}

public class ConnectionRequestMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.ConnectionRequest;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (!isServerHandled)
        {
            throw new ExpectedServerException();
        }

        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<ConnectionRequestData>();

        // Make sure the id isn't spoofed.
        if (NetworkInfo.IsSpoofed(data.longId))
        {
            ConnectionSender.SendConnectionDeny(data.longId, "Your player ID does not match the networked ID.");
            return;
        }

        var newSmallId = PlayerIdManager.GetUnusedPlayerId();

        // No unused ids available
        if (!newSmallId.HasValue)
        {
            ConnectionSender.SendConnectionDeny(data.longId, "Server ran out of space! Wait for someone to leave.");
            return;
        }

        // Player already is in the server?
        if (PlayerIdManager.GetPlayerId(data.longId) != null)
        {
            ConnectionSender.SendConnectionDeny(data.longId, "You attempted to join, but the server detects you as already in it?");
        }

        // If the connection request is invalid, deny it
        if (!data.IsValid)
        {
            ConnectionSender.SendConnectionDeny(data.longId, "Connection request was invalid. You are likely on mismatching versions.");
            return;
        }

        // Check if theres too many players
        if (PlayerIdManager.PlayerCount >= byte.MaxValue || PlayerIdManager.PlayerCount >= SavedServerSettings.MaxPlayers.Value)
        {
            ConnectionSender.SendConnectionDeny(data.longId, "Server is full! Wait for someone to leave.");
            return;
        }

        // Make sure we aren't loading
        if (FusionSceneManager.IsLoading())
        {
            ConnectionSender.SendConnectionDeny(data.longId, "Host is loading.");
            return;
        }

        // Verify joining
        bool isVerified = NetworkVerification.IsClientApproved(data.longId);

        if (!isVerified)
        {
            ConnectionSender.SendConnectionDeny(data.longId, "Server is private.");
            return;
        }

        // Compare versions
        VersionResult versionResult = NetworkVerification.CompareVersion(FusionMod.Version, data.version);

        if (versionResult != VersionResult.Ok)
        {
            switch (versionResult)
            {
                default:
                case VersionResult.Unknown:
                    ConnectionSender.SendConnectionDeny(data.longId, "Unknown Version Mismatch");
                    break;
                case VersionResult.Lower:
                    ConnectionSender.SendConnectionDeny(data.longId, "Server is on an older version. Downgrade your version or notify the host.");
                    break;
                case VersionResult.Higher:
                    ConnectionSender.SendConnectionDeny(data.longId, "Server is on a newer version. Update your version.");
                    break;
            }

            return;
        }

        // Get the permission level
        FusionPermissions.FetchPermissionLevel(data.longId, out var level, out _);

        // Check for banning
        if (NetworkHelper.IsBanned(data.longId))
        {
            ConnectionSender.SendConnectionDeny(data.longId, "Banned from Server");
            return;
        }

        // Append metadata with info
        data.initialMetadata[MetadataHelper.PermissionKey] = level.ToString();

        // Create new PlayerID
        var playerId = new PlayerId(data.longId, newSmallId.Value, data.initialMetadata, data.initialEquippedItems);

        // Finally, check for dynamic connection disallowing
        if (!MultiplayerHooking.Internal_OnShouldAllowConnection(playerId, out string reason))
        {
            ConnectionSender.SendConnectionDeny(data.longId, reason);
            return;
        }

        // All checks have succeeded, let the player into the server
        OnConnectionAllowed(playerId, data);
    }

    private static void OnConnectionAllowed(PlayerId playerId, ConnectionRequestData data)
    {
        // First we send the new player to all existing players (and the new player so they know they exist)
        ConnectionSender.SendPlayerJoin(playerId, data.avatarBarcode, data.avatarStats);

        // Now we send all of our other players to the new player
        foreach (var id in PlayerIdManager.PlayerIds)
        {
            var barcode = CommonBarcodes.INVALID_AVATAR_BARCODE;
            SerializedAvatarStats stats = new();

            if (id.SmallId == PlayerIdManager.HostSmallId)
            {
                barcode = RigData.RigAvatarId;
                stats = RigData.RigAvatarStats;
            }
            else if (NetworkPlayerManager.TryGetPlayer(id.SmallId, out var rep))
            {
                barcode = rep.AvatarSetter.AvatarBarcode;
                stats = rep.AvatarSetter.AvatarStats;
            }

            ConnectionSender.SendPlayerCatchup(data.longId, id, barcode, stats);
        }

        // Now, make sure the player loads into the scene
        LoadSender.SendLevelLoad(FusionSceneManager.Barcode, FusionSceneManager.LoadBarcode, data.longId);

        // Send the dynamics list
        using (var writer = FusionWriter.Create())
        {
            var assignData = DynamicsAssignData.Create();
            writer.Write(assignData);

            using var message = FusionMessage.Create(NativeMessageTag.DynamicsAssignment, writer);
            MessageSender.SendFromServer(data.longId, NetworkChannel.Reliable, message);
        }

        // Send the active server settings
        LobbyInfoManager.SendLobbyInfo(data.longId);

        // SERVER CATCHUP
        // Catchup hooked events
        MultiplayerHooking.Internal_OnPlayerCatchup(playerId);
    }
}