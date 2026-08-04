using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.UI.Popups;
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

    public override ServerID RunningServerID => _runningServerID;

    public override bool IsClientConnected => ClientSteamConnection != null;

    public override ServerID ConnectedServerID => _connectedServerID;

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

    private ServerID _runningServerID = ServerID.Empty;
    private ServerID _connectedServerID = ServerID.Empty;

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

        ServerSteamSocket.ServerSendToClients(clientPlatformIDs, channel, message);
    }

    public override void ClientSendToServer(NetMessage message, NetworkChannel channel)
    {
        if (!IsClientConnected)
        {
            return;
        }

        ClientSteamConnection.ClientSendToServer(channel, message);
    }

    public override void StartServer()
    {
        ServerSteamSocket = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>();
        _runningServerID = new ServerID(ClientSteamID);

        RefreshServerCode();

        InvokeServerStartedEvent();
    }

    public override void StopServer()
    {
        if (!IsServerRunning)
        {
            return;
        }

        try
        {
            ServerSteamSocket?.Close();
        }
        catch (Exception e)
        {
            FusionLogger.LogException("stopping server", e);
        }

        ServerSteamSocket = null;
        _runningServerID = ServerID.Empty;

        InvokeServerStoppedEvent();
    }

    public override void ServerDisconnectClient(ClientPlatformID client)
    {
        if (!IsServerRunning)
        {
            return;
        }

        ServerSteamSocket.DisconnectUser((ulong)client);
    }

    public override void ConnectToServer(ServerID server)
    {
        if (IsClientConnected)
        {
            ClientDisconnectFromServer();
        }

        SteamId serverSteamID = (ulong)server;

        ClientSteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverSteamID);
        _connectedServerID = server;

        InvokeConnectionEstablishedEvent();
    }

    public override void ClientDisconnectFromServer()
    {
        if (!IsClientConnected)
        {
            return;
        }

        try
        {
            ClientSteamConnection?.Close();
        }
        catch (Exception e)
        {
            FusionLogger.LogException("disconnecting client from server", e);
        }

        ClientSteamConnection = null;
        _connectedServerID = ServerID.Empty;

        InvokeConnectionLostEvent();
    }

    public override void Disconnect(string reason = "")
    {
        throw new NotImplementedException();
    }

    public override void DisconnectUser(ClientPlatformID platformID)
    {
        throw new NotImplementedException();
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

            ConnectToServer(info.Lobbies[0].Metadata.LobbyInfo.LobbyID);
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