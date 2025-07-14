using Epic.OnlineServices;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.P2P;

using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Voice;
using LabFusion.Voice.Unity;

using MelonLoader;

namespace LabFusion.Network;

public enum LobbyConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Disconnecting
}

public class EOSNetworkLayer : NetworkLayer
{
    protected bool _isInitialized = false;

    public override string Title => "Epic Online Services";

    public override string Platform => "Epic";

    public override int MaxPlayers => 64;

    public override int MaxLobbies => 100;

    public override bool RequiresValidId => true;

    public override bool IsHost => _isServerActive;
    public override bool IsClient => _isConnectionActive;

    private EOSLobby _currentLobby;
    public override INetworkLobby Lobby => _currentLobby;

    private IVoiceManager _voiceManager = null;
    public override IVoiceManager VoiceManager => _voiceManager;

    private IMatchmaker _matchmaker = null;
    public override IMatchmaker Matchmaker => _matchmaker;

    protected bool _isServerActive = false;
    protected bool _isConnectionActive = false;

    // EOS Log Level
    internal static LogLevel LogLevel => LogLevel.Warning;

    // EOS Lobby Details
    internal LobbyDetails LobbyDetails => _currentLobby?.LobbyDetails;

    // Current connection state. Needs a rework/better solution
    public LobbyConnectionState LobbyConnectionState { get; private set; } = LobbyConnectionState.Disconnected;

    // Different player IDs used across the layer
    public static ProductUserId LocalUserId;
    public static EpicAccountId LocalAccountId;
    internal static ProductUserId HostId = null;

    // Notification IDs for handling different connection events
    private ulong connectionRequestedId = Common.InvalidNotificationid;
    private ulong connectionEstablishedId = Common.InvalidNotificationid;
    private ulong connectionClosedId = Common.InvalidNotificationid;

    public override bool CheckSupported()
    {
        return true;
    }

    public override bool CheckValidation()
    {
        return EOSSDKLoader.HasEOSSDK;
    }

    public override void LogIn()
    {
        NetworkLayerNotifications.SendLoggingInNotification();

        MelonCoroutines.Start(EOSManager.InitEOS((success) =>
        {
            if (success)
                InvokeLoggedInEvent();
            else
            {
                FusionLogger.Error("Failed to log in to EOS Network Layer.");
                NetworkLayerNotifications.SendLoginFailedNotification();
            }
        }));
    }

    public override void LogOut()
    {
        InvokeLoggedOutEvent();
    }

    public override void OnInitializeLayer()
    {
        try
        {
            PlayerIDManager.SetStringID(LocalUserId.ToString());
            LocalPlayer.Username = GetUsername(LocalUserId.ToString());

            _voiceManager = new UnityVoiceManager();
            _voiceManager.Enable();

            _matchmaker = new EOSMatchmaker();

            HookEvents();

            _isInitialized = true;
        }
        catch (Exception e)
        {
            FusionLogger.Error($"Failed to initialize EOS Network Layer: {e.Message}");
            _isInitialized = false;
        }
    }

    public override void OnDeinitializeLayer()
    {
        Disconnect();

        _voiceManager?.Disable();
        _voiceManager = null;

        _matchmaker = null;
        _currentLobby = null;

        SetLobbyConnectionState(LobbyConnectionState.Disconnected);

        EOSManager.ShutdownEOS();

        UnhookEvents();

        _isInitialized = false;
    }

    public override string GetUsername(string userId)
    {
        return EOSUtils.GetDisplayNameFromProductId(ProductUserId.FromString(userId));
    }

    public override bool IsFriend(string userId)
    {
        if (userId == LocalUserId.ToString())
            return true;

        EpicAccountId epicAccountId = EOSUtils.GetAccountIdFromProductId(ProductUserId.FromString(userId));
        var getStatusOptions = new GetStatusOptions
        {
            LocalUserId = LocalAccountId,
            TargetUserId = epicAccountId
        };

        return EOSManager.FriendsInterface.GetStatus(ref getStatusOptions) == FriendsStatus.Friends;
    }

    private void HookEvents()
    {
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
        MultiplayerHooking.OnDisconnected += OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;
    }

    private void AddNotifyPeerEvents()
    {
        if (IsHost)
        {
            var requestOptions = new AddNotifyPeerConnectionRequestOptions
            {
                SocketId = EOSSocketHandler.SocketId,
                LocalUserId = LocalUserId
            };
            var closedOptions = new AddNotifyPeerConnectionClosedOptions
            {
                SocketId = EOSSocketHandler.SocketId,
                LocalUserId = LocalUserId
            };

            connectionRequestedId = EOSManager.P2PInterface.AddNotifyPeerConnectionRequest(ref requestOptions, null, (ref OnIncomingConnectionRequestInfo callbackInfo) =>
            {
                var acceptOptions = new AcceptConnectionOptions
                {
                    RemoteUserId = callbackInfo.RemoteUserId,
                    SocketId = EOSSocketHandler.SocketId,
                    LocalUserId = LocalUserId
                };
                EOSManager.P2PInterface.AcceptConnection(ref acceptOptions);
            });
            connectionClosedId = EOSManager.P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, (ref OnRemoteConnectionClosedInfo info) =>
            {
                var closeOptions = new CloseConnectionOptions
                {
                    RemoteUserId = info.RemoteUserId,
                    SocketId = EOSSocketHandler.SocketId,
                    LocalUserId = LocalUserId
                };
                if (PlayerIDManager.HasPlayerID(info.RemoteUserId.ToString()))
                {
                    InternalServerHelpers.OnPlayerLeft(info.RemoteUserId.ToString());

                    ConnectionSender.SendDisconnect(info.RemoteUserId.ToString());
                }
                EOSManager.P2PInterface.CloseConnection(ref closeOptions);
            });
        }
        else
        {
            var establishedOptions = new AddNotifyPeerConnectionEstablishedOptions
            {
                SocketId = EOSSocketHandler.SocketId,
                LocalUserId = LocalUserId
            };
            var closedOptions = new AddNotifyPeerConnectionClosedOptions
            {
                SocketId = EOSSocketHandler.SocketId,
                LocalUserId = LocalUserId
            };

            connectionEstablishedId = EOSManager.P2PInterface.AddNotifyPeerConnectionEstablished(ref establishedOptions, null, (ref OnPeerConnectionEstablishedInfo data) =>
            {
                ConnectionSender.SendConnectionRequest();

                SetLobbyConnectionState(LobbyConnectionState.Connected);
            });
            connectionClosedId = EOSManager.P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, (ref OnRemoteConnectionClosedInfo info) =>
            {
                // Disconnect when the host closes the lobby
                Disconnect();
            });
        }
    }

    private void UnhookEvents()
    {
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeave;
        MultiplayerHooking.OnDisconnected -= OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged -= OnUpdateLobby;
    }

    private void RemoveNotifyPeerEvents()
    {
        if (IsHost)
        {
            RemoveNotification(ref connectionRequestedId, EOSManager.P2PInterface.RemoveNotifyPeerConnectionRequest);
            RemoveNotification(ref connectionClosedId, EOSManager.P2PInterface.RemoveNotifyPeerConnectionClosed);
        }
        else
        {
            RemoveNotification(ref connectionEstablishedId, EOSManager.P2PInterface.RemoveNotifyPeerConnectionEstablished);
            RemoveNotification(ref connectionClosedId, EOSManager.P2PInterface.RemoveNotifyPeerConnectionClosed);
        }
    }

    private void RemoveNotification(ref ulong notificationId, Action<ulong> removeAction)
    {
        if (notificationId != Common.InvalidNotificationid)
        {
            removeAction(notificationId);
            notificationId = Common.InvalidNotificationid;
        }
    }

    public override void OnUpdateLayer()
    {
        if (_isConnectionActive)
            EOSSocketHandler.ReceiveMessages();
    }

    public void OnUpdateLobby()
    {
        if (_currentLobby == null)
        {
            return;
        }

        LobbyMetadataSerializer.WriteInfo(Lobby);

        var copyOptions = new CopyLobbyDetailsHandleOptions
        {
            LobbyId = _currentLobby.LobbyId,
            LocalUserId = LocalUserId
        };

        EOSManager.LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);

        _currentLobby.UpdateLobbyDetails(lobbyDetails);
    }

    private void OnPlayerJoin(PlayerID id)
    {
        if (VoiceManager == null || id.IsMe)
            return;

        VoiceManager.GetSpeaker(id);
    }

    private void OnPlayerLeave(PlayerID id)
    {
        VoiceManager?.RemoveSpeaker(id);
    }

    private void SetLobbyConnectionState(LobbyConnectionState newState)
    {
        if (LobbyConnectionState != newState)
        {
            var previousState = LobbyConnectionState;
            LobbyConnectionState = newState;
            FusionLogger.Log($"Lobby connection state changed: {previousState} -> {newState}");
        }
    }

    public override void StartServer()
    {
        if (!_isInitialized || LobbyConnectionState == LobbyConnectionState.Connecting)
            return;

        SetLobbyConnectionState(LobbyConnectionState.Connecting);
        CreateEpicLobby();
    }

    private void CreateEpicLobby()
    {
        var createOptions = new CreateLobbyOptions
        {
            BucketId = "FUSION",
            DisableHostMigration = true,
            LocalUserId = LocalUserId,
            MaxLobbyMembers = 64,
            PermissionLevel = LobbyPermissionLevel.Publicadvertised,
            EnableRTCRoom = false,
            PresenceEnabled = true,
            RejoinAfterKickRequiresInvite = false,
            EnableJoinById = true,
            AllowInvites = true,
        };
        EOSManager.LobbyInterface.CreateLobby(ref createOptions, null, (ref CreateLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
                FusionLogger.Error($"Failed to create EOS lobby: {info.ResultCode}");
                SetLobbyConnectionState(LobbyConnectionState.Disconnected);
                return;
            }

            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = info.LobbyId,
                LocalUserId = LocalUserId
            };
            EOSManager.LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);

            _isServerActive = true;
            _isConnectionActive = true;

            _currentLobby = new EOSLobby(lobbyDetails, info.LobbyId);

            HostId = LocalUserId;

            InternalServerHelpers.OnStartServer();
            SetLobbyConnectionState(LobbyConnectionState.Connected);

            AddNotifyPeerEvents();

#if DEBUG
            FusionLogger.Log($"Created EOS lobby: {info.ResultCode} with ID {info.LobbyId}");
#endif

            RefreshServerCode();
        });
    }

    public override void Disconnect(string reason = "")
    {
        if (!_isServerActive && !_isConnectionActive)
            return;

        SetLobbyConnectionState(LobbyConnectionState.Disconnecting);

        if (IsHost)
            DestroyLobby(reason);
        else
            LeaveLobby(reason);
    }

    private void DestroyLobby(string reason)
    {
        var destroyOptions = new DestroyLobbyOptions
        {
            LocalUserId = LocalUserId,
            LobbyId = _currentLobby.LobbyId
        };

        EOSManager.LobbyInterface.DestroyLobby(ref destroyOptions, null, (ref DestroyLobbyCallbackInfo info) => OnDisconnectComplete(reason));
    }

    private void LeaveLobby(string reason)
    {
        var leaveOptions = new LeaveLobbyOptions
        {
            LobbyId = _currentLobby.LobbyId,
            LocalUserId = LocalUserId
        };

        EOSManager.LobbyInterface.LeaveLobby(ref leaveOptions, null, (ref LeaveLobbyCallbackInfo info) => OnDisconnectComplete(reason));
    }

    private void OnDisconnectComplete(string reason)
    {
        RemoveNotifyPeerEvents();
        EOSConnectionManager.Close();
        _isServerActive = false;
        _isConnectionActive = false;
        _currentLobby = null;
        ServerCode = null;
        InternalServerHelpers.OnDisconnect(reason);
        SetLobbyConnectionState(LobbyConnectionState.Disconnected);
    }

    private void OnDisconnect()
    {
        VoiceManager?.ClearManager();
    }

    public void JoinServer(string lobbyId)
    {
        if (_isConnectionActive || _isServerActive)
            Disconnect();

        // we can return if we are connected to a lobby. we should have already disconnected/started to when we run Disconnect above
        // may cause the user to need to click join twice, but that is also a thing on steam so idddrrrcccc
        if (LobbyConnectionState is LobbyConnectionState.Connecting or LobbyConnectionState.Connected)
            return;

        SetLobbyConnectionState(LobbyConnectionState.Connecting);

        var joinLobbyOptions = new JoinLobbyByIdOptions
        {
            CrossplayOptOut = false,
            LobbyId = lobbyId,
            LocalUserId = LocalUserId,
            PresenceEnabled = true,
        };
        EOSManager.LobbyInterface.JoinLobbyById(ref joinLobbyOptions, null, (ref JoinLobbyByIdCallbackInfo joinDelegate) =>
        {
            if (joinDelegate.ResultCode != Result.Success)
            {
                FusionLogger.Log(lobbyId);
                FusionLogger.Error($"Failed to join EOS lobby: {joinDelegate.ResultCode}");
                SetLobbyConnectionState(LobbyConnectionState.Disconnected);
                return;
            }

            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = joinDelegate.LobbyId,
                LocalUserId = LocalUserId
            };
            EOSManager.LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);

            var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            HostId = lobbyDetails.GetLobbyOwner(ref ownerOptions);

            _isServerActive = false;
            _isConnectionActive = true;

            _currentLobby = new EOSLobby(lobbyDetails, joinDelegate.LobbyId);

            // Add events so once we have a connection, we join on the fusion end
            AddNotifyPeerEvents();

            // Send a dummy packet to establish the connection
            NetMessage message = NetMessage.Create(0, Array.Empty<byte>(), CommonMessageRoutes.None);
            EOSSocketHandler.SendPacketToUser(HostId, message, NetworkChannel.Reliable, false);
        });
    }

    public string ServerCode { get; private set; } = null;

    public override string GetServerCode() => ServerCode;

    public override void RefreshServerCode()
    {
        ServerCode = RandomCodeGenerator.GetString(8);

        LobbyInfoManager.PushLobbyUpdate();
    }

    public override void JoinServerByCode(string code)
    {
#if DEBUG
        FusionLogger.Log($"Searching for servers with code {code}...");
#endif

        Matchmaker.RequestLobbiesByCode(code, (info) =>
        {
            if (info.Lobbies.Length <= 0)
            {
                FusionLogger.Log("No lobbies found with the given code.");
                return;
            }

            JoinServer(info.Lobbies[0].Metadata.LobbyInfo.LobbyId);
        });
    }

    public override string GetServerID() => _currentLobby.LobbyId;

    public override void BroadcastMessage(NetworkChannel channel, NetMessage message)
    {
        if (IsHost)
            EOSSocketHandler.BroadcastToClients(channel, message);
        else
            EOSSocketHandler.BroadcastToServer(channel, message);
    }

    public override void SendToServer(NetworkChannel channel, NetMessage message)
    {
        EOSSocketHandler.BroadcastToServer(channel, message);
    }

    public override void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
    {
        EOSSocketHandler.SendFromServer(userId, channel, message);
    }

    public override void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
    {
        var playerID = PlayerIDManager.GetPlayerID(userId);
        if (playerID != null)
        {
            SendFromServer(playerID.PlatformID, channel, message);
        }
    }
}