using Epic.OnlineServices;
using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Voice;
using LabFusion.Voice.Unity;
using MelonLoader;

namespace LabFusion.Network;

// TODO:
// Try fixing server trying to handle a few left over messages when kicking somebody from server
// Fix stuttery voice
// Handle when joining a lobby fails
// Remove lobby stuff. Only use lobby for matchmaking   
public class EpicGamesNetworkLayer : NetworkLayer
{
    private const int ServerCodeLength = 8;

    public override string Title => "Epic Online Services";
    public override string Platform => "Epic";
    public override bool IsHost => _isServerActive;
    public override bool IsClient => _isConnectionActive;
    
    public override INetworkLobby Lobby => Runtime.Lobby.CurrentLobby;

    private IVoiceManager _voiceManager = null;
    public override IVoiceManager VoiceManager => _voiceManager;
    
    private IMatchmaker _matchmaker = null;
    public override IMatchmaker Matchmaker => _matchmaker;

    private bool _isServerActive;
    private bool _isConnectionActive;
    private string _serverCode = string.Empty;
    private bool _joinInProgress;
    
    internal EOSRuntime Runtime;
    internal ProductUserId LocalUserId => Runtime.Connect.LocalUserId;

    public override bool CheckSupported() => true;

    public override bool CheckValidation() => EOSSDKLoader.HasEOSSDK;

    public override void LogIn()
    {
        NetworkLayerNotifications.SendLoggingInNotification();
        
        Runtime = new EOSRuntime();
        
        MelonCoroutines.Start(Runtime.InitializeAsync((success) => 
        {
            if (success)
            {
                InvokeLoggedInEvent();
            }
            else
            {
                NetworkLayerNotifications.SendLoginFailedNotification();
                InvokeLoggedOutEvent();
            }
        }));
    }

    public override void LogOut()
    {
        InvokeLoggedOutEvent();
    }

    public override void OnInitializeLayer()
    { 
        // Get EOS information
        PlayerIDManager.SetPlatformID(LocalUserId.ToString());
        LocalPlayer.Username = Runtime.Connect.LocalDisplayName;
        
        FusionLogger.Log($"EOS initialized with ProductUserId {LocalUserId.ToString()}!");
        
        HookEvents();
        
        // Create managers
        _voiceManager = new UnityVoiceManager();
        _voiceManager.Enable();
        
        _matchmaker = new EpicMatchmaker(Runtime, LocalUserId);
    }

    public override void OnDeinitializeLayer()
    {
        _voiceManager.Disable();
        _voiceManager = null;

        _matchmaker = null;
        
        Disconnect();
        
        UnhookEvents();
        
        Runtime.Shutdown();
    }

    public override void OnUpdateLayer()
    {
        if (_isConnectionActive)
        {
            Runtime.P2P.Receiver.Receive();
        }
    }

    // This method doesn't actually get used anywhere in fusion. However, the steam layer uses it for setting the local username.
    // EOS doesn't have a way to do this synchronously, so we just return a dummy username.
    public override string GetUsername(string userId)
    {
        return "FusionPlayer";
    }

    // EOS doesn't have a friends system when using device Ids.
    public override bool IsFriend(string userId)
    {
        return userId != null && userId == LocalUserId.ToString();
    }

    public override void BroadcastMessage(NetworkChannel channel, NetMessage message)
    {
        if (IsHost)
        {
            foreach (var peer in Runtime.P2P.ConnectedPeers)
            {
                Runtime.P2P.Sender.Send(peer, message, channel, false);
            }
        }
        else
        {
            Runtime.P2P.Sender.Send(Runtime.Lobby.CurrentLobby.Owner, message, channel, true);
        }
    }

    public override void SendToServer(NetworkChannel channel, NetMessage message)
    {
        Runtime.P2P.Sender.Send(Runtime.Lobby.CurrentLobby.Owner, message, channel, true);
    }

    public override void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
    {
        var id = PlayerIDManager.GetPlayerID(userId);
        if (id != null)
        {
            SendFromServer(id.PlatformID, channel, message);
        }
    }

    public override void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
    {
        // Make sure this is actually the server
        if (!IsHost)
        {
            return;
        }
        
        Runtime.P2P.Sender.Send(ProductUserId.FromString(userId), message, channel, false);
    }

    public override void StartServer()
    {
        Runtime.P2P.RegisterHostNotifications();
        
        Runtime.Lobby.CreateLobby(OnFailed);
        Runtime.P2P.AddConnectedPeer(LocalUserId);
        
        _isServerActive = true;
        _isConnectionActive = true;
        
        InternalServerHelpers.OnStartServer();

        RefreshServerCode();

        void OnFailed()
        {
            Disconnect();
        }
    }
    
    internal void JoinServer(EpicLobby epicLobby)
    {
        if (_joinInProgress)
        {
            FusionLogger.Warn("Join lobby already in progress");
            return;
        }
        
        _joinInProgress = true;
        
        if (_isConnectionActive || _isServerActive)
            Disconnect();
        
        Runtime.P2P.RegisterClientNotifications();
        Runtime.P2P.OnConnected += OnConnected;
        
        Runtime.Lobby.JoinLobby(epicLobby, OnFailed);
        Runtime.P2P.Connect(epicLobby.Owner);
        
        _isServerActive = false;
        _isConnectionActive = true;

        void OnConnected(ProductUserId remoteUserId)
        {
            Runtime.P2P.OnConnected = null;
            _joinInProgress = false;
            ConnectionSender.SendConnectionRequest();
        }

        void OnFailed()
        {
            Disconnect();
        }
    }

    public override void Disconnect(string reason = "")
    {
        if (!_isServerActive && !_isConnectionActive)
            return;
        
        _joinInProgress = false;
        
        Runtime.P2P.UnregisterAllNotifications();
        Runtime.P2P.OnConnected = null;
        
        Runtime.Lobby.LeaveLobby();
        Runtime.P2P.Disconnect();
        
        _isServerActive = false;
        _isConnectionActive = false;

        InternalServerHelpers.OnDisconnect(reason);
    }

    public override void DisconnectUser(string platformID)
    {
        if (!_isServerActive)
        {
            return;
        }
        
        Runtime.P2P.DisconnectUser(ProductUserId.FromString(platformID));
    }

    public override string GetServerCode() => _serverCode;

    public override void RefreshServerCode()
    {
        _serverCode = RandomCodeGenerator.GetString(ServerCodeLength);
        LobbyInfoManager.PushLobbyUpdate();
    }

    public override string GetServerID()
    {
        return Runtime.Lobby.CurrentLobby?.LobbyID?.ToString() ?? string.Empty;
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

            if (info.Lobbies[0].Lobby is EpicLobby epicLobby)
            {
                JoinServer(epicLobby);
            }
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
        if (_voiceManager != null && !id.IsMe)
        {
            _voiceManager.GetSpeaker(id);
        }
    }

    private void OnPlayerLeave(PlayerID id)
    {
        _voiceManager?.RemoveSpeaker(id);
    }

    private void OnDisconnect()
    {
        _voiceManager?.ClearManager();
    }

    private void OnUpdateLobby()
    {
        var lobby = Runtime.Lobby.CurrentLobby;
        if (lobby == null || lobby.Owner != LocalUserId)
            return;
        
        LobbyMetadataSerializer.WriteInfo(lobby);
    }
}
