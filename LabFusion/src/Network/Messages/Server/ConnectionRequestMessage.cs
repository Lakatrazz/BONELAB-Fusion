using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Preferences.Server;
using LabFusion.Senders;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Safety;

namespace LabFusion.Network;

public class ConnectionRequestData : INetSerializable
{
    public ulong PlatformID;
    public Version Version;
    public string AvatarBarcode;
    public SerializedAvatarStats AvatarStats;
    public Dictionary<string, string> InitialMetadata;
    public List<string> InitialEquippedItems;

    public int? GetSize() => sizeof(ulong) + Version.GetSize() + AvatarBarcode.GetSize() + SerializedAvatarStats.Size + InitialMetadata.GetSize() + InitialEquippedItems.GetSize();

    public bool IsValid { get; private set; } = true;

    public void Serialize(INetSerializer serializer)
    {
        try
        {
            serializer.SerializeValue(ref PlatformID);
            serializer.SerializeValue(ref Version);
            serializer.SerializeValue(ref AvatarBarcode);
            serializer.SerializeValue(ref AvatarStats);
            serializer.SerializeValue(ref InitialMetadata);
            serializer.SerializeValue(ref InitialEquippedItems);
        }
        catch (Exception e)
        {
            IsValid = false;

            FusionLogger.LogException("serializing ConnectionRequestData", e);
        }
    }

    public static ConnectionRequestData Create(ulong longId, Version version, string avatarBarcode, SerializedAvatarStats stats)
    {
        LocalPlayer.InvokeApplyInitialMetadata();

        return new ConnectionRequestData()
        {
            PlatformID = longId,
            Version = version,
            AvatarBarcode = avatarBarcode,
            AvatarStats = stats,
            InitialMetadata = LocalPlayer.Metadata.Metadata.LocalDictionary,
            InitialEquippedItems = InternalServerHelpers.GetInitialEquippedItems(),
        };
    }
}

public class ConnectionRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ConnectionRequest;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConnectionRequestData>();

        // Make sure the id isn't spoofed.
        if (NetworkInfo.IsSpoofed(data.PlatformID))
        {
            ConnectionSender.SendConnectionDeny(data.PlatformID, "Your player ID does not match the networked ID.");
            return;
        }

        var newSmallId = PlayerIDManager.GetUniquePlayerID();

        // No unused ids available
        if (!newSmallId.HasValue)
        {
            ConnectionSender.SendConnectionDeny(data.PlatformID, "Server ran out of space! Wait for someone to leave.");
            return;
        }

        // Player already is in the server?
        if (PlayerIDManager.GetPlayerID(data.PlatformID) != null)
        {
            ConnectionSender.SendConnectionDeny(data.PlatformID, "You attempted to join, but the server detects you as already in it?");
        }

        // If the connection request is invalid, deny it
        if (!data.IsValid)
        {
            ConnectionSender.SendConnectionDeny(data.PlatformID, "Connection request was invalid. You are likely on mismatching versions.");
            return;
        }

        // Check if theres too many players
        if (PlayerIDManager.PlayerCount >= byte.MaxValue || PlayerIDManager.PlayerCount >= SavedServerSettings.MaxPlayers.Value)
        {
            ConnectionSender.SendConnectionDeny(data.PlatformID, "Server is full! Wait for someone to leave.");
            return;
        }

        // Make sure we aren't loading
        if (FusionSceneManager.IsLoading())
        {
            ConnectionSender.SendConnectionDeny(data.PlatformID, "Host is loading.");
            return;
        }

        // Verify joining
        bool isVerified = NetworkVerification.IsClientApproved(data.PlatformID);

        if (!isVerified)
        {
            ConnectionSender.SendConnectionDeny(data.PlatformID, "Server is private.");
            return;
        }

        // Compare versions
        VersionResult versionResult = NetworkVerification.CompareVersion(FusionMod.Version, data.Version);

        if (versionResult != VersionResult.Ok)
        {
            switch (versionResult)
            {
                default:
                case VersionResult.Unknown:
                    ConnectionSender.SendConnectionDeny(data.PlatformID, "Unknown Version Mismatch");
                    break;
                case VersionResult.Lower:
                    ConnectionSender.SendConnectionDeny(data.PlatformID, "Server is on an older version. Downgrade your version or notify the host.");
                    break;
                case VersionResult.Higher:
                    ConnectionSender.SendConnectionDeny(data.PlatformID, "Server is on a newer version. Update your version.");
                    break;
            }

            return;
        }

        // Get the permission level
        FusionPermissions.FetchPermissionLevel(data.PlatformID, out var level, out _);

        // Check for banning
        if (NetworkHelper.IsBanned(data.PlatformID))
        {
            ConnectionSender.SendConnectionDeny(data.PlatformID, "Banned from Server");
            return;
        }

        // Check for global banning
        var globalBanInfo = GlobalBanManager.GetBanInfo(new PlatformInfo(data.PlatformID));

        if (globalBanInfo != null && SavedServerSettings.Privacy.Value != ServerPrivacy.FRIENDS_ONLY)
        {
            ConnectionSender.SendConnectionDeny(data.PlatformID, globalBanInfo.Reason);
            return;
        }

        // Append metadata with info
        data.InitialMetadata[nameof(PlayerMetadata.PermissionLevel)] = level.ToString();

        // Create new PlayerID
        var playerId = new PlayerID(data.PlatformID, newSmallId.Value, data.InitialMetadata, data.InitialEquippedItems);

        // Finally, check for dynamic connection disallowing
        if (!MultiplayerHooking.CheckShouldAllowConnection(playerId, out string reason))
        {
            ConnectionSender.SendConnectionDeny(data.PlatformID, reason);
            return;
        }

        // All checks have succeeded, let the player into the server
        OnConnectionAllowed(playerId, data);
    }

    private static void OnConnectionAllowed(PlayerID playerID, ConnectionRequestData data)
    {
        // Reserve the player's smallID so that other players don't steal it
        PlayerIDManager.ReserveSmallID(playerID.SmallID);

        // Send the new player to all existing players (and the new player so they know they exist)
        ConnectionSender.SendPlayerJoin(playerID, data.AvatarBarcode, data.AvatarStats);

        // Now we send all of our other players to the new player
        foreach (var id in PlayerIDManager.PlayerIDs)
        {
            // Don't resend the new player to themselves
            if (id.SmallID == playerID.SmallID)
            {
                continue;
            }

            string barcode;
            SerializedAvatarStats stats;

            if (id.SmallID == PlayerIDManager.HostSmallID)
            {
                barcode = RigData.RigAvatarId;
                stats = RigData.RigAvatarStats;
            }
            else if (NetworkPlayerManager.TryGetPlayer(id.SmallID, out var rep))
            {
                barcode = rep.AvatarSetter.AvatarBarcode;
                stats = rep.AvatarSetter.AvatarStats;
            }
            else
            {
                continue;
            }

            ConnectionSender.SendPlayerCatchup(data.PlatformID, id, barcode, stats);
        }

        // Now, make sure the player loads into the scene
        LoadSender.SendLevelLoad(FusionSceneManager.Barcode, FusionSceneManager.LoadBarcode, data.PlatformID);

        // Send the dynamics list
        var assignData = DynamicsAssignData.Create();

        using (var writer = NetWriter.Create(assignData.GetSize()))
        {
            assignData.Serialize(writer);

            using var message = NetMessage.Create(NativeMessageTag.DynamicsAssignment, writer, CommonMessageRoutes.None);
            MessageSender.SendFromServer(data.PlatformID, NetworkChannel.Reliable, message);
        }

        // Send the active server settings
        LobbyInfoManager.SendLobbyInfo(data.PlatformID);
    }
}