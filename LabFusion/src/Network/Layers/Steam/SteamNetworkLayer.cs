using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.UI.Popups;

using Steamworks;
using Steamworks.Data;

using LabFusion.Senders;
using LabFusion.Voice;
using LabFusion.Voice.Unity;

namespace LabFusion.Network;

public abstract class SteamNetworkLayer : NetworkLayer
{
    public abstract uint ApplicationID { get; }

    public const int ReceiveBufferSize = 32;

    public override string Title => "Steam";

    public override string Platform => "Steam";

    public override bool RequiresValidId => true;

    public override bool IsHost => _isServerActive;
    public override bool IsClient => _isConnectionActive;

    private INetworkLobby _currentLobby;
    public override INetworkLobby Lobby => _currentLobby;

    private IVoiceManager _voiceManager = null;
    public override IVoiceManager VoiceManager => _voiceManager;

    private IMatchmaker _matchmaker = null;
    public override IMatchmaker Matchmaker => _matchmaker;

    public SteamId SteamId;

    public static SteamSocketManager SteamSocket;
    public static SteamConnectionManager SteamConnection;

    protected bool _isServerActive = false;
    protected bool _isConnectionActive = false;

    protected ulong _targetServerId;

    protected string _targetJoinId;

    protected bool _isInitialized = false;

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
        SteamId = SteamClient.SteamId;
        PlayerIDManager.SetLongID(SteamId.Value);
        LocalPlayer.Username = GetUsername(SteamId.Value);

        FusionLogger.Log($"Steamworks initialized with SteamID {SteamId} and ApplicationID {ApplicationID}!");

        SteamNetworkingUtils.InitRelayNetworkAccess();

        HookSteamEvents();

        // Create managers
        _voiceManager = new UnityVoiceManager();
        _voiceManager.Enable();

        _matchmaker = new SteamMatchmaker();

        // Set initialized
        _isInitialized = true;
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
            SteamSocket?.Receive(ReceiveBufferSize);

            SteamConnection?.Receive(ReceiveBufferSize);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("receiving data on Socket and Connection", e);
        }
    }

    public override string GetUsername(ulong userId)
    {
        return new Friend(userId).Name;
    }

    public override bool IsFriend(ulong userId)
    {
        return userId == PlayerIDManager.LocalPlatformID || new Friend(userId).IsFriend;
    }

    public override void BroadcastMessage(NetworkChannel channel, NetMessage message)
    {
        if (IsHost)
        {
            SteamSocketHandler.BroadcastToClients(SteamSocket, channel, message);
        }
        else
        {
            SteamSocketHandler.BroadcastToServer(channel, message);
        }
    }

    public override void SendToServer(NetworkChannel channel, NetMessage message)
    {
        SteamSocketHandler.BroadcastToServer(channel, message);
    }

    public override void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
    {
        var id = PlayerIDManager.GetPlayerID(userId);

        if (id != null)
        {
            SendFromServer(id.PlatformID, channel, message);
        }
    }

    public override void SendFromServer(ulong userId, NetworkChannel channel, NetMessage message)
    {
        // Make sure this is actually the server
        if (!IsHost)
        {
            return;
        }

        // Get the connection from the userid dictionary
        if (SteamSocket.ConnectedSteamIds.TryGetValue(userId, out var connection))
        {
            SteamSocket.SendToClient(connection, channel, message);
        }
    }

    public override void StartServer()
    {
        SteamSocket = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);

        // Host needs to connect to own socket server with a ConnectionManager to send/receive messages
        // Relay Socket servers are created/connected to through SteamIds rather than "Normal" Socket Servers which take IP addresses
        SteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(SteamId);
        _isServerActive = true;
        _isConnectionActive = true;

        // Call server setup
        InternalServerHelpers.OnStartServer();

        RefreshServerCode();
    }

    public void JoinServer(SteamId serverId)
    {
        // Leave existing server
        if (_isConnectionActive || _isServerActive)
            Disconnect();

        SteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverId, 0);

        _isServerActive = false;
        _isConnectionActive = true;

        ConnectionSender.SendConnectionRequest();
    }

    public override void Disconnect(string reason = "")
    {
        // Make sure we are currently in a server
        if (!_isServerActive && !_isConnectionActive)
            return;

        try
        {
            SteamConnection?.Close();

            SteamSocket?.Close();
        }
        catch
        {
            FusionLogger.Log("Error closing socket server / connection manager");
        }

        _isServerActive = false;
        _isConnectionActive = false;

        InternalServerHelpers.OnDisconnect(reason);
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

            JoinServer(info.Lobbies[0].Metadata.LobbyInfo.LobbyId);
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
        if (_localLobby.Id == SteamId)
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