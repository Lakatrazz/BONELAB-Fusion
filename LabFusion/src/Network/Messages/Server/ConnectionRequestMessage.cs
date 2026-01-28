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
    public string BackupPlatformID;
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
            serializer.SerializeValue(ref BackupPlatformID);
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

    public static ConnectionRequestData Create(string stringID, Version version, string avatarBarcode, SerializedAvatarStats stats)
    {
        LocalPlayer.InvokeApplyInitialMetadata();

        return new ConnectionRequestData()
        {
            BackupPlatformID = stringID,
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

        string platformID = received.PlatformID ?? data.BackupPlatformID;

        var newSmallId = PlayerIDManager.GetUniquePlayerID();

        // No unused ids available
        if (!newSmallId.HasValue)
        {
            ConnectionSender.SendConnectionDeny(platformID, "Server ran out of space! Wait for someone to leave.");
            return;
        }

        // Player already is in the server?
        if (PlayerIDManager.GetPlayerID(platformID) != null)
        {
            ConnectionSender.SendConnectionDeny(platformID, "You attempted to join, but the server detects you as already in it?");
            return;
        }

        // If the connection request is invalid, deny it
        if (!data.IsValid)
        {
            ConnectionSender.SendConnectionDeny(platformID, "Connection request was invalid. You are likely on mismatching versions.");
            return;
        }

        // Check if theres too many players
        if (PlayerIDManager.PlayerCount >= byte.MaxValue || PlayerIDManager.PlayerCount >= SavedServerSettings.MaxPlayers.Value)
        {
            ConnectionSender.SendConnectionDeny(platformID, "Server is full! Wait for someone to leave.");
            return;
        }

        // Make sure we aren't loading
        if (FusionSceneManager.IsLoading())
        {
            ConnectionSender.SendConnectionDeny(platformID, "Host is loading.");
            return;
        }

        // Verify joining
        bool isVerified = NetworkVerification.IsClientApproved(platformID);

        if (!isVerified)
        {
            ConnectionSender.SendConnectionDeny(platformID, "Server is private.");
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
                    ConnectionSender.SendConnectionDeny(platformID, "Unknown Version Mismatch");
                    break;
                case VersionResult.Lower:
                    ConnectionSender.SendConnectionDeny(platformID, "Server is on an older version. Downgrade your version or notify the host.");
                    break;
                case VersionResult.Higher:
                    ConnectionSender.SendConnectionDeny(platformID, "Server is on a newer version. Update your version.");
                    break;
            }

            return;
        }

        // Get the permission level
        FusionPermissions.FetchPermissionLevel(platformID, out var level, out _);

        // Check for banning
        if (NetworkHelper.IsBanned(platformID))
        {
            ConnectionSender.SendConnectionDeny(platformID, "Banned from Server");
            return;
        }

        // Check for global banning
        var globalBanInfo = GlobalBanManager.GetBanInfo(new PlatformInfo(platformID));

        if (globalBanInfo != null && SavedServerSettings.Privacy.Value != ServerPrivacy.FRIENDS_ONLY)
        {
            ConnectionSender.SendConnectionDeny(platformID, globalBanInfo.Reason);
            return;
        }

        // Append metadata with info
        data.InitialMetadata[nameof(PlayerMetadata.PermissionLevel)] = level.ToString();

        // Create new PlayerID
        var playerId = new PlayerID(platformID, newSmallId.Value, data.InitialMetadata, data.InitialEquippedItems);

        // Finally, check for dynamic connection disallowing
        if (!MultiplayerHooking.CheckShouldAllowConnection(playerId, out string reason))
        {
            ConnectionSender.SendConnectionDeny(platformID, reason);
            return;
        }

        // All checks have succeeded, let the player into the server
        OnConnectionAllowed(playerId, platformID, data);
    }

    private static void OnConnectionAllowed(PlayerID playerID, string platformID, ConnectionRequestData data)
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

            ConnectionSender.SendPlayerCatchup(platformID, id, barcode, stats);
        }

        // Now, make sure the player loads into the scene
        LoadSender.SendLevelLoad(FusionSceneManager.Barcode, FusionSceneManager.LoadBarcode, platformID);

        // Send the dynamics list
        var assignData = DynamicsAssignData.Create();

        using (var writer = NetWriter.Create(assignData.GetSize()))
        {
            assignData.Serialize(writer);

            using var message = NetMessage.Create(NativeMessageTag.DynamicsAssignment, writer, CommonMessageRoutes.None);
            MessageSender.SendFromServer(platformID, NetworkChannel.Reliable, message);
        }

        // Send the active server settings
        LobbyInfoManager.SendLobbyInfo(platformID);
    }
}