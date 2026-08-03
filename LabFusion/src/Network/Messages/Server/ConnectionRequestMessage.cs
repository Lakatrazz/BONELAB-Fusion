using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Preferences.Server;
using LabFusion.Senders;
using LabFusion.Network.Serialization;
using LabFusion.Safety;
using LabFusion.Network.Messages;

namespace LabFusion.Network;

public class ConnectionRequestData : INetSerializable
{
    public Version Version;

    public Dictionary<string, string> InitialMetadata;

    public int? GetSize() => Version.GetSize() + InitialMetadata.GetSize();

    public bool IsValid { get; private set; } = true;

    public void Serialize(INetSerializer serializer)
    {
        try
        {
            serializer.SerializeValue(ref Version);

            serializer.SerializeValue(ref InitialMetadata);
        }
        catch (Exception e)
        {
            IsValid = false;

            FusionLogger.LogException("serializing ConnectionRequestData", e);
        }
    }

    public static ConnectionRequestData Create(Version version)
    {
        LocalPlayer.InvokeApplyInitialMetadata();

        return new ConnectionRequestData()
        {
            Version = version,
            InitialMetadata = LocalPlayer.Metadata.Metadata.LocalDictionary,
        };
    }
}

public class ConnectionRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ConnectionRequest;

    // Only the server should be able to receive a connection request.
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    // When a client sends a request to connect, they do not have an established PlayerID yet, so a direct relay must be allowed.
    public override bool AllowDirectRelay => true;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConnectionRequestData>();

        if (!received.SenderPlatformID.HasValue)
        {
            FusionLogger.Error("A client attempted to connect, but ReceivedMessage.PlatformID was not set! Make sure that a unique ID is being passed in for connecting clients!");
            return;
        }

        ClientPlatformID platformID = received.SenderPlatformID.Value;

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
        var playerId = new PlayerID(platformID, newSmallId.Value, data.InitialMetadata);

        // Finally, check for dynamic connection disallowing
        if (!MultiplayerHooking.CheckShouldAllowConnection(playerId, out string reason))
        {
            ConnectionSender.SendConnectionDeny(platformID, reason);
            return;
        }

        // All checks have succeeded, let the player into the server
        OnConnectionAllowed(playerId, platformID);
    }

    private static void OnConnectionAllowed(PlayerID playerID, ClientPlatformID platformID)
    {
        // Reserve the player's smallID so that other players don't steal it
        PlayerIDManager.ReserveSmallID(playerID.SmallID);

        // Send the new player to all existing players (and the new player so they know they exist)
        ConnectionSender.SendPlayerJoin(playerID);

        // Now we send all of our other players to the new player
        foreach (var id in PlayerIDManager.PlayerIDs)
        {
            // Don't resend the new player to themselves
            if (id.SmallID == playerID.SmallID)
            {
                continue;
            }

            ConnectionSender.SendPlayerCatchup(platformID, id);
        }

        // Now, make sure the player loads into the scene
        LoadSender.SendLevelLoad(FusionSceneManager.Barcode, FusionSceneManager.LoadBarcode, platformID);

        // Send the dynamics list
        using var message = MessageCreator.CreateNative(DynamicsAssignData.Create(), NativeMessageTag.DynamicsAssignment, CommonMessageRoutes.None);

        ServerManager.SendToClient(message, NetworkChannel.Reliable, platformID);

        // Send the active server settings
        LobbyInfoManager.SendLobbyInfo(platformID);
    }
}