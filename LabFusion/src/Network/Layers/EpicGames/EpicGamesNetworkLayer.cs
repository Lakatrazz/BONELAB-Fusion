using Epic.OnlineServices;

using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Voice;
using LabFusion.Voice.Unity;

using MelonLoader;

namespace LabFusion.Network.EpicGames;

public class EpicGamesNetworkLayer : NetworkLayer
{
    private const int ServerCodeLength = 8;

    public override string Title => "Epic Online Services";
    public override string Platform => "Epic";
    public override bool IsHost => _isServerActive;
    public override bool IsClient => _isConnectionActive;

    public override INetworkLobby Lobby => _lobbyManager?.CurrentLobby;

    private IVoiceManager _voiceManager;
    public override IVoiceManager VoiceManager => _voiceManager;

    private IMatchmaker _matchmaker;
    public override IMatchmaker Matchmaker => _matchmaker;

    private bool _isServerActive;
    private bool _isConnectionActive;
    private string _serverCode = string.Empty;

    private EOSManager _eosManager;
    private EOSAuthManager _authManager;
    private EOSLobbyManager _lobbyManager;
    private EOSP2PManager _p2pManager;
    private EOSConnectionStateManager  _connectionStateManager;
    private EOSConnectionHandler _connectionHandler;
    private EOSNotificationManager _notificationManager;

    private ProductUserId LocalUserId => _authManager?.LocalUserId;

    public override bool CheckSupported() => true;

    public override bool CheckValidation() => EOSSDKLoader.HasEOSSDK;

    public override void LogIn()
    {
        _authManager ??= new EOSAuthManager();
        _eosManager ??= new EOSManager(_authManager);

        NetworkLayerNotifications.SendLoggingInNotification();

        MelonCoroutines.Start(_eosManager.InitializeAsync(OnLoginComplete));
    }

    private void OnLoginComplete(bool success)
    {
        if (success)
        {
            InvokeLoggedInEvent();
        }
        else
        {
            FusionLogger.Error("Failed to log in to EOS Network Layer.");
            NetworkLayerNotifications.SendLoginFailedNotification();
        }
    }

    public override void LogOut()
    {
        InvokeLoggedOutEvent();
    }

    public override void OnInitializeLayer()
    {
        if (LocalUserId == null)
        {
            FusionLogger.Error("Cannot initialize layer: LocalUserId is null");
            return;
        }

        // Set player identity
        PlayerIDManager.SetPlatformID(LocalUserId.ToString());

        MelonCoroutines.Start(EOSUsernameDeterminer.GetUsernameAsync(username =>
        {
            LocalPlayer.Username = username;
        }));

        // Initialize managers
        InitializeManagers();

        // Hook events
        HookEvents();

        // Initialize voice
        _voiceManager = new UnityVoiceManager();
        _voiceManager.Enable();

        // Initialize matchmaking
        _matchmaker = new EOSMatchmaker();
    }

    public override void OnDeinitializeLayer()
    {
        Disconnect();

        _voiceManager?.Disable();
        _voiceManager = null;

        _matchmaker = null;

        CleanupManagers();

        UnhookEvents();

        _eosManager?.Shutdown();
        _eosManager = null;
        _authManager = null;
    }

    public override void OnUpdateLayer()
    {
        if (_isConnectionActive)
        {
            EOSMessenger.ReceiveMessages();
        }
    }

    private void InitializeManagers()
    {
        _lobbyManager = new EOSLobbyManager(LocalUserId);
        _p2pManager = new EOSP2PManager(LocalUserId, EOSMessenger.SocketId);
        _connectionStateManager = new EOSConnectionStateManager();
        _connectionHandler = new EOSConnectionHandler(_p2pManager, _connectionStateManager, OnDisconnectedFromHost);
        _notificationManager = new EOSNotificationManager(LocalUserId, EOSMessenger.SocketId, _connectionHandler);

        _p2pManager.Configure();
    }

    private void CleanupManagers()
    {
        _notificationManager?.UnregisterAllNotifications();
        _notificationManager = null;
        _connectionStateManager = null;
        _connectionHandler = null;
        _p2pManager = null;
        _lobbyManager = null;

        EOSMessenger.Reset();
    }

    public override string GetUsername(string platformId)
    {
        return EOSUsernameDeterminer.GetRandomUsername(platformId);
    }

    public override bool IsFriend(string userId)
    {
        // Device ID auth doesn't support friends
        return false;
    }

    public override void BroadcastMessage(NetworkChannel channel, NetMessage message)
    {
        if (IsHost)
            EOSMessenger.BroadcastToClients(channel, message);
        else
            EOSMessenger.BroadcastToServer(channel, message);
    }

    public override void SendToServer(NetworkChannel channel, NetMessage message)
    {
        EOSMessenger.BroadcastToServer(channel, message);
    }

    public override void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
    {
        var playerID = PlayerIDManager.GetPlayerID(userId);
        if (playerID != null)
        {
            SendFromServer(playerID.PlatformID, channel, message);
        }
    }

    public override void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
    {
        EOSMessenger.SendFromServer(userId, channel, message);
    }

    public override void StartServer()
    {
        if (LocalUserId == null)
        {
            FusionLogger.Error("Cannot start server: LocalUserId is null");
            return;
        }
        
        if (!_connectionStateManager.CanStartServer())
            return;

        _connectionStateManager.SetConnectionState(EOSConnectionStateManager.ConnectionState.Connecting);
        
        _lobbyManager.CreateLobby(OnLobbyCreated);
    }

    private void OnLobbyCreated(EpicLobby lobby)
    {
        if (lobby == null)
        {
            FusionLogger.Error("Failed to create lobby");
            _connectionStateManager.SetConnectionState(EOSConnectionStateManager.ConnectionState.Disconnected);
            return;
        }

        _isServerActive = true;
        _isConnectionActive = true;

        InternalServerHelpers.OnStartServer();
        
        _connectionStateManager.SetConnectionState(EOSConnectionStateManager.ConnectionState.Connected);

        _notificationManager.RegisterHostNotifications();

        RefreshServerCode();

#if DEBUG
        FusionLogger.Log($"Server started with lobby: {lobby.LobbyId}");
#endif
    }

    public void JoinServer(string lobbyId)
    {
        if (string.IsNullOrEmpty(lobbyId))
        {
            FusionLogger.Error("Cannot join server: lobbyId is null or empty");
            return;
        }

        if (_isConnectionActive || _isServerActive)
            Disconnect();
        
        if (!_connectionStateManager.CanJoinServer())
            return;
        
        _connectionStateManager.SetConnectionState(EOSConnectionStateManager.ConnectionState.Connecting);

        if (LocalUserId == null)
        {
            FusionLogger.Error("Cannot join server: LocalUserId is null");
            return;
        }

        _lobbyManager.JoinLobby(lobbyId, OnLobbyJoined);
    }

    private void OnLobbyJoined(EpicLobby lobby)
    {
        if (lobby == null)
        {
            FusionLogger.Error("Failed to join lobby");
            _connectionStateManager.SetConnectionState(EOSConnectionStateManager.ConnectionState.Disconnected);
            return;
        }

        _isServerActive = false;
        _isConnectionActive = true;

        _notificationManager.RegisterClientNotifications();

        // Send a dummy packet to establish the P2P connection
        var hostId = _lobbyManager.GetLobbyOwner();
        if (hostId != null)
        {
            var message = NetMessage.Create(0, Array.Empty<byte>(), CommonMessageRoutes.None);
            EOSMessenger.SendPacket(hostId, message, NetworkChannel.Reliable, isServerHandled: false);
        }

#if DEBUG
        FusionLogger.Log($"Joined lobby: {lobby.LobbyId}");
#endif
    }

    public override void Disconnect(string reason = "")
    {
        if (!_isServerActive && !_isConnectionActive)
            return;
        
        _connectionStateManager.SetConnectionState(EOSConnectionStateManager.ConnectionState.Disconnecting);

        if (IsHost)
        {
            _lobbyManager?.DestroyLobby(() => OnDisconnectComplete(reason));
        }
        else
        {
            _lobbyManager?.LeaveLobby(() => OnDisconnectComplete(reason));
        }
    }

    private void OnDisconnectedFromHost()
    {
        Disconnect("Lobby closed");
    }

    private void OnDisconnectComplete(string reason)
    {
        _notificationManager?.UnregisterAllNotifications();
        _p2pManager?.CloseAllConnections();

        _isServerActive = false;
        _isConnectionActive = false;
        _serverCode = string.Empty;

        InternalServerHelpers.OnDisconnect(reason);
        
        _connectionStateManager.SetConnectionState(EOSConnectionStateManager.ConnectionState.Disconnected);

#if DEBUG
        FusionLogger.Log($"Disconnected: {(string.IsNullOrEmpty(reason) ? "No reason" : reason)}");
#endif
    }

    public override void DisconnectUser(string platformID)
    {
        if (!_isServerActive)
            return;

        _lobbyManager?.KickMember(platformID);
    }

    public override string GetServerCode() => _serverCode;

    public override void RefreshServerCode()
    {
        _serverCode = RandomCodeGenerator.GetString(ServerCodeLength);
        LobbyInfoManager.PushLobbyUpdate();
    }

    public override string GetServerID() => _lobbyManager?.CurrentLobby?.LobbyId ?? string.Empty;

    public override void JoinServerByCode(string code)
    {
        if (Matchmaker == null)
            return;

#if DEBUG
        FusionLogger.Log($"Searching for servers with code {code}.. .");
#endif

        Matchmaker.RequestLobbiesByCode(code, info =>
        {
            if (info.Lobbies.Length <= 0)
            {
                FusionLogger.Log("No lobbies found with the given code.");
                return;
            }

            JoinServer(info.Lobbies[0].Metadata.LobbyInfo.LobbyID);
        });
    }

    private void HookEvents()
    {
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
        MultiplayerHooking.OnDisconnected += OnDisconnect;
        LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;
    }

    private void UnhookEvents()
    {
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeave;
        MultiplayerHooking.OnDisconnected -= OnDisconnect;
        LobbyInfoManager.OnLobbyInfoChanged -= OnUpdateLobby;
    }

    private void OnPlayerJoin(PlayerID id)
    {
        if (VoiceManager != null && !id.IsMe)
        {
            VoiceManager.GetSpeaker(id);
        }
    }

    private void OnPlayerLeave(PlayerID id)
    {
        VoiceManager?.RemoveSpeaker(id);
    }

    private void OnDisconnect()
    {
        VoiceManager?.ClearManager();
    }

    private void OnUpdateLobby()
    {
        if (Lobby == null)
            return;

        LobbyMetadataSerializer.WriteInfo(Lobby);
    }
}