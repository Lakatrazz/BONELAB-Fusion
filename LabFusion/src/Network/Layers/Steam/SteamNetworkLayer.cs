using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.UI.Popups;
using LabFusion.Senders;
using LabFusion.Voice;
using LabFusion.Voice.Unity;

using Steamworks;
using Steamworks.Data;

namespace LabFusion.Network;

public abstract class SteamNetworkLayer : NetworkLayer
{
    public abstract uint ApplicationID { get; }

    public const int ReceiveBufferSize = 32;

    public override string Title => "Steam";

    public override string Platform => "Steam";

    public override bool IsServerRunning => ServerSteamSocket != null;

    public override bool IsClientConnected => ClientSteamConnection != null;

    private INetworkLobby _currentLobby;
    public override INetworkLobby Lobby => _currentLobby;

    private IVoiceManager _voiceManager = null;
    public override IVoiceManager VoiceManager => _voiceManager;

    private IMatchmaker _matchmaker = null;
    public override IMatchmaker Matchmaker => _matchmaker;

    /// <summary>
    /// The steam client's logged in SteamID.
    /// </summary>
    public static SteamId ClientSteamID { get; private set; }

    /// <summary>
    /// The server's steam socket manager, if a server is running.
    /// </summary>
    public static SteamSocketManager ServerSteamSocket { get; private set; } = null;

    /// <summary>
    /// The client's steam connection manager, if a client is connected to a server.
    /// </summary>
    public static SteamConnectionManager ClientSteamConnection { get; private set; } = null;

    // A local reference to a lobby
    // This isn't actually used for joining servers, just for matchmaking
    protected Lobby _localLobby;

    public override bool CheckSupported()
    {
        return !PlatformHelper.IsAndroid;
    }

    public override bool CheckValidation()
    {
        return SteamAPILoader.HasSteamAPI;
    }

    public override void OnInitializeLayer()
    {
        if (!SteamClient.IsValid)
        {
            FusionLogger.Error("Steamworks failed to initialize!");
            return;
        }

        // Get steam information
        ClientSteamID = SteamClient.SteamId;

        var platformID = new ClientPlatformID(ClientSteamID.Value);

        PlayerIDManager.SetPlatformID(platformID);
        LocalPlayer.Username = GetUsername(platformID);

        FusionLogger.Log($"Steamworks initialized with SteamID {ClientSteamID} and ApplicationID {ApplicationID}!");

        SteamNetworkingUtils.InitRelayNetworkAccess();

        HookSteamEvents();

        // Create managers
        _voiceManager = new UnityVoiceManager();
        _voiceManager.Enable();

        _matchmaker = new SteamMatchmaker();
    }

    public override void OnDeinitializeLayer()
    {
        _voiceManager.Disable();
        _voiceManager = null;

        _matchmaker = null;

        _localLobby = default;
        _currentLobby = null;

        Disconnect();

        UnHookSteamEvents();

        SteamAPI.Shutdown();
    }

    public override void LogIn()
    {
        if (SteamClient.IsValid)
        {
            return;
        }

        // Shutdown the game's steam client, if available
        if (GameHasSteamworks())
        {
            ShutdownGameClient();
        }

        bool succeeded;

        try
        {
            SteamClient.Init(ApplicationID, false);

            succeeded = true;
        }
        catch (Exception e)
        {
            FusionLogger.LogException("initializing Steamworks", e);

            succeeded = false;
        }

        if (!succeeded)
        {
            Notifier.Send(new Notification()
            {
                Title = "Log In Failed",
                Message = "Failed connecting to Steamworks! Make sure Steam is running and signed in!",
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.ERROR,
                PopupLength = 6f,
            });

            InvokeLoggedOutEvent();
            return;
        }

        InvokeLoggedInEvent();
    }

    public override void LogOut()
    {
        SteamClient.Shutdown();

        InvokeLoggedOutEvent();
    }

    private const string STEAMWORKS_ASSEMBLY_NAME = "Il2CppFacepunch.Steamworks.Win64";

    private static bool GameHasSteamworks()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            if (assembly.FullName.StartsWith(STEAMWORKS_ASSEMBLY_NAME))
            {
                return true;
            }
        }

        return false;
    }

    private static void ShutdownGameClient()
    {
        FusionLogger.Log("Shutting down the game's Steamworks instance...");

        Il2CppSteamworks.SteamClient.Shutdown();
    }

    public override void OnUpdateLayer()
    {
        // Run callbacks for our client
        SteamClient.RunCallbacks();

        // Receive any needed messages
        try
        {
            ServerSteamSocket?.Receive(ReceiveBufferSize);

            ClientSteamConnection?.Receive(ReceiveBufferSize);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("receiving data on Socket and Connection", e);
        }
    }

    public override string GetUsername(ClientPlatformID platformID)
    {
        return new Friend((ulong)platformID).Name;
    }

    public override bool IsFriend(ClientPlatformID platformID)
    {
        return platformID == PlayerIDManager.LocalPlatformID || new Friend((ulong)platformID).IsFriend;
    }

    public override void ServerSendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID)
    {
        if (!IsServerRunning)
        {
            return;
        }

        ServerSteamSocket.SendToClient(clientPlatformID, channel, message);
    }

    public override void ServerSendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clientPlatformIDs)
    {
        if (!IsServerRunning)
        {
            return;
        }

        ServerSteamSocket.SendToClients(clientPlatformIDs, channel, message);
    }

    public override void ClientSendToServer(NetMessage message, NetworkChannel channel)
    {
        SteamSocketHandler.BroadcastToServer(channel, message);
    }

    public override void StartServer()
    {
        ServerSteamSocket = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);

        // Host needs to connect to own socket server with a ConnectionManager to send/receive messages
        // Relay Socket servers are created/connected to through SteamIds rather than "Normal" Socket Servers which take IP addresses
        ClientSteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(ClientSteamID);

        // Call server setup
        InternalServerHelpers.OnStartServer();

        RefreshServerCode();
    }

    public void JoinServer(SteamId serverId)
    {
        // Leave existing server
        if (IsClientConnected || IsServerRunning)
        {
            Disconnect();
        }

        ClientSteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverId, 0);

        ConnectionSender.SendConnectionRequest();
    }

    public override void Disconnect(string reason = "")
    {
        // Make sure we are currently in a server
        if (!IsServerRunning && !IsClientConnected)
        {
            return;
        }

        try
        {
            ClientSteamConnection?.Close();

            ServerSteamSocket?.Close();
        }
        catch
        {
            FusionLogger.Log("Error closing socket server / connection manager");
        }

        InternalServerHelpers.OnDisconnect(reason);
    }

    public override void DisconnectUser(ClientPlatformID platformID)
    {
        // Make sure we are hosting a server
        if (!IsServerRunning)
        {
            return;
        }

        ServerSteamSocket.DisconnectUser((ulong)platformID);
    }

    public string ServerCode { get; private set; } = null;

    public override string GetServerCode()
    {
        return ServerCode;
    }

    public override void RefreshServerCode()
    {
        ServerCode = RandomCodeGenerator.GetString(8);

        LobbyInfoManager.PushLobbyUpdate();
    }

    public override void JoinServerByCode(string code)
    {
        if (Matchmaker == null)
        {
            return;
        }

#if DEBUG
        FusionLogger.Log($"Searching for servers with code {code}...");
#endif

        Matchmaker.RequestLobbiesByCode(code, (info) =>
        {
            if (info.Lobbies.Length <= 0)
            {
                return;
            }

            JoinServer((ulong)info.Lobbies[0].Metadata.LobbyInfo.LobbyID);
        });
    }

    private void HookSteamEvents()
    {
        // Add server hooks
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
        MultiplayerHooking.OnDisconnected += OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;

        // Create a local lobby
        AwaitLobbyCreation();
    }

    private void OnPlayerJoin(PlayerID id)
    {
        if (VoiceManager == null)
        {
            return;
        }

        if (!id.IsMe)
        {
            VoiceManager.GetSpeaker(id);
        }
    }

    private void OnPlayerLeave(PlayerID id)
    {
        if (VoiceManager == null)
        {
            return;
        }

        VoiceManager.RemoveSpeaker(id);
    }

    private void OnDisconnect()
    {
        if (VoiceManager == null)
        {
            return;
        }

        VoiceManager.ClearManager();
    }

    private void UnHookSteamEvents()
    {
        // Remove server hooks
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeave;
        MultiplayerHooking.OnDisconnected -= OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged -= OnUpdateLobby;

        // Remove the local lobby
        if (_localLobby.Id == ClientSteamID)
        {
            _localLobby.Leave();
        }
    }

    private async void AwaitLobbyCreation()
    {
        var lobbyTask = await SteamMatchmaking.CreateLobbyAsync();

        if (!lobbyTask.HasValue)
        {
#if DEBUG
            FusionLogger.Log("Failed to create a steam lobby!");
#endif
            return;
        }

        _localLobby = lobbyTask.Value;
        _currentLobby = new SteamLobby(_localLobby);
    }

    public void OnUpdateLobby()
    {
        // Make sure the lobby exists
        if (Lobby == null)
        {
#if DEBUG
            FusionLogger.Warn("Tried updating the steam lobby, but it was null!");
#endif
            return;
        }

        // Write active info about the lobby
        LobbyMetadataSerializer.WriteInfo(Lobby);
    }
}