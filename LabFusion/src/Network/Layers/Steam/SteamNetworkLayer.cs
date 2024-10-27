using BoneLib.BoneMenu;

using UnityEngine;

using Il2CppWebSocketSharp;

using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Scene;

using Steamworks;
using Steamworks.Data;

using Color = UnityEngine.Color;

using LabFusion.Senders;
using LabFusion.BoneMenu;
using LabFusion.Voice;
using LabFusion.Voice.Unity;

namespace LabFusion.Network;

public abstract class SteamNetworkLayer : NetworkLayer
{
    public abstract uint ApplicationID { get; }

    public const int ReceiveBufferSize = 32;

    public override string Title => "Steam";

    public override bool RequiresValidId => true;

    public override bool IsServer => _isServerActive;
    public override bool IsClient => _isConnectionActive;

    private INetworkLobby _currentLobby;
    public override INetworkLobby CurrentLobby => _currentLobby;

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
        // Make sure the API actually loaded
        if (!SteamAPILoader.HasSteamAPI)
        {
            return false;
        }

        try
        {
            // Try loading the steam client
            if (!SteamClient.IsValid)
            {
                SteamClient.Init(ApplicationID, false);
            }

            return true;
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"initializing {Title} layer", e);
            return false;
        }
    }

    public override void OnInitializeLayer()
    {
        try
        {
            if (!SteamClient.IsValid)
            {
                SteamClient.Init(ApplicationID, false);
            }
        }
        catch (Exception e)
        {
            FusionLogger.Error($"Failed to initialize Steamworks! \n{e}");
        }

        _voiceManager = new UnityVoiceManager();
        _voiceManager.Enable();

        _matchmaker = new SteamMatchmaker();
    }

    public override void OnLateInitializeLayer()
    {
        if (!SteamClient.IsValid)
        {
            FusionLogger.Log("Steamworks failed to initialize!");
            return;
        }

        SteamId = SteamClient.SteamId;
        PlayerIdManager.SetLongId(SteamId.Value);
        PlayerIdManager.SetUsername(GetUsername(SteamId.Value));

        FusionLogger.Log($"Steamworks initialized with SteamID {SteamId} and ApplicationID {ApplicationID}!");

        SteamNetworkingUtils.InitRelayNetworkAccess();

        HookSteamEvents();

        _isInitialized = true;
    }

    public override void OnCleanupLayer()
    {
        Disconnect();

        UnHookSteamEvents();

        _voiceManager.Disable();
        _voiceManager = null;

        SteamAPI.Shutdown();
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
        return userId == PlayerIdManager.LocalLongId || new Friend(userId).IsFriend;
    }

    public override void BroadcastMessage(NetworkChannel channel, FusionMessage message)
    {
        if (IsServer)
        {
            SteamSocketHandler.BroadcastToClients(SteamSocket, channel, message);
        }
        else
        {
            SteamSocketHandler.BroadcastToServer(channel, message);
        }
    }

    public override void SendToServer(NetworkChannel channel, FusionMessage message)
    {
        SteamSocketHandler.BroadcastToServer(channel, message);
    }

    public override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message)
    {
        var id = PlayerIdManager.GetPlayerId(userId);

        if (id != null)
        {
            SendFromServer(id.LongId, channel, message);
        }
    }

    public override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message)
    {
        // Make sure this is actually the server
        if (!IsServer)
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

        Matchmaker.RequestLobbies((info) =>
        {
            foreach (var lobby in info.lobbies)
            {
                var lobbyCode = lobby.metadata.LobbyInfo.LobbyCode;
                var inputCode = code;

                // Case insensitive
                // Makes it easier to input
                if (lobbyCode.ToLower() == code.ToLower())
                {
                    JoinServer(lobby.metadata.LobbyInfo.LobbyId);
                    break;
                }
            }
        });
    }

    private void HookSteamEvents()
    {
        // Add server hooks
        MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
        MultiplayerHooking.OnDisconnect += OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;

        // Create a local lobby
        AwaitLobbyCreation();
    }

    private void OnPlayerJoin(PlayerId id)
    {
        if (!id.IsMe)
        {
            VoiceManager.GetSpeaker(id);
        }
    }

    private void OnPlayerLeave(PlayerId id)
    {
        VoiceManager.RemoveSpeaker(id);
    }

    private void OnDisconnect()
    {
        VoiceManager.ClearManager();
    }

    private void UnHookSteamEvents()
    {
        // Remove server hooks
        MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
        MultiplayerHooking.OnDisconnect -= OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged -= OnUpdateLobby;

        // Remove the local lobby
        _localLobby.Leave();
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
        if (CurrentLobby == null)
        {
#if DEBUG
            FusionLogger.Warn("Tried updating the steam lobby, but it was null!");
#endif
            return;
        }

        // Write active info about the lobby
        LobbyMetadataHelper.WriteInfo(CurrentLobby);
    }
}