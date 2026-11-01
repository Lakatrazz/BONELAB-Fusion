using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Preferences.Server;
using LabFusion.Senders;
using LabFusion.Network.Serialization;
using LabFusion.Safety;
using LabFusion.Moderation;

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

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    public override bool AllowConnectingClients => true;

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

        bool isListenServer = ClientManager.IsClientConnecting && platformID == PlayerIDManager.LocalPlatformID;

        // No unused ids available
        if (!newSmallId.HasValue)
        {
            ServerManager.SendDisconnect(platformID, "Server ran out of space! Wait for someone to leave.");
            return;
        }

        // Player already is in the server?
        if (PlayerIDManager.GetPlayerID(platformID) != null)
        {
            ServerManager.SendDisconnect(platformID, "You attempted to join, but the server detects you as already in it?");
            return;
        }

        // If the connection request is invalid, deny it
        if (!data.IsValid)
        {
            ServerManager.SendDisconnect(platformID, "Connection request was invalid. You are likely on mismatching versions.");
            return;
        }

        // Check if theres too many players
        if (PlayerIDManager.PlayerCount >= byte.MaxValue || PlayerIDManager.PlayerCount >= SavedServerSettings.MaxPlayers.Value)
        {
            ServerManager.SendDisconnect(platformID, "Server is full! Wait for someone to leave.");
            return;
        }

        // Make sure we aren't loading
        if (!isListenServer && FusionSceneManager.IsLoading())
        {
            ServerManager.SendDisconnect(platformID, "Host is loading.");
            return;
        }

        // Verify joining
        bool isVerified = NetworkVerification.IsClientApproved(platformID);

        if (!isVerified)
        {
            ServerManager.SendDisconnect(platformID, "Server is private.");
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
                    ServerManager.SendDisconnect(platformID, "Unknown Version Mismatch");
                    break;
                case VersionResult.Lower:
                    ServerManager.SendDisconnect(platformID, "Server is on an older version. Downgrade your version or notify the host.");
                    break;
                case VersionResult.Higher:
                    ServerManager.SendDisconnect(platformID, "Server is on a newer version. Update your version.");
                    break;
            }

            return;
        }

        // Get the permission level
        FusionPermissions.FetchPermissionLevel(platformID, out var level, out _);

        // Check for banning
        if (ModerationManager.IsBanned(platformID))
        {
            ServerManager.SendDisconnect(platformID, "Banned from Server");
            return;
        }

        // Check for global banning
        var globalBanInfo = GlobalBanManager.GetBanInfo(new PlatformInfo(platformID));

        if (globalBanInfo != null && SavedServerSettings.Privacy.Value != ServerPrivacy.FRIENDS_ONLY)
        {
            ServerManager.SendDisconnect(platformID, globalBanInfo.Reason);
            return;
        }

        // Append metadata with info
        data.InitialMetadata[nameof(PlayerMetadata.PermissionLevel)] = level.ToString();

        // Create new PlayerID
        PlayerIDManager.RegisterPlayer(platformID, newSmallId.Value, data.InitialMetadata, true, out var playerID);

        // All checks have succeeded, let the player into the server
        OnConnectionAllowed(playerID, platformID);
    }

    private static void OnConnectionAllowed(PlayerID playerID, ClientPlatformID platformID)
    {
        BroadcastConnection(playerID);

        CatchupConnections(platformID);

        // Now, make sure the player loads into the scene
        LoadSender.SendLevelLoad(FusionSceneManager.Barcode, FusionSceneManager.LoadBarcode, platformID);

        // Send the dynamics list
        using var message = NetMessage.CreateNative(DynamicsAssignData.Create(), NativeMessageTag.DynamicsAssignment, CommonMessageRoutes.None);

        ServerManager.SendToClient(message, NetworkChannel.Reliable, platformID);

        // Send the active server settings
        LobbyInfoManager.SendLobbyInfo(platformID);
    }

    private static void BroadcastConnection(PlayerID playerID)
    {
        var response = new ConnectionResponseData()
        {
            PlatformID = playerID.PlatformID,
            SmallID = playerID.SmallID,
            InitialMetadata = playerID.Metadata.Metadata.LocalDictionary,
            IsJoining = true,
        };

        ServerManager.SendToClientsNative(response, NativeMessageTag.ConnectionResponse, NetworkChannel.Reliable);
    }

    private static void CatchupConnections(ClientPlatformID client)
    {
        foreach (var playerID in PlayerIDManager.PlayerIDs)
        {
            if (playerID.PlatformID == client)
            {
                continue;
            }

            CatchupConnection(client, playerID);
        }
    }

    private static void CatchupConnection(ClientPlatformID client, PlayerID existingPlayerID)
    {
        var response = new ConnectionResponseData()
        {
            PlatformID = existingPlayerID.PlatformID,
            SmallID = existingPlayerID.SmallID,
            InitialMetadata = existingPlayerID.Metadata.Metadata.LocalDictionary,
            IsJoining = false,
        };

        ServerManager.SendToClientNative(response, NativeMessageTag.ConnectionResponse, NetworkChannel.Reliable, client);
    }
}