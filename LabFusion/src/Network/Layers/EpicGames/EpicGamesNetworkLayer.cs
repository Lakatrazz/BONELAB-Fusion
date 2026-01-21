using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.P2P;
using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Voice;
using LabFusion.Voice.Unity;

using MelonLoader;

namespace LabFusion.Network.EpicGames;

public class EpicGamesNetworkLayer : NetworkLayer
{
    public override string Title => "Epic Online Services";

    public override string Platform => "Epic";

    public override bool IsHost => _isServerActive;
    public override bool IsClient => _isConnectionActive;

    private EpicLobby _currentLobby;
    public override INetworkLobby Lobby => _currentLobby;

    private IVoiceManager _voiceManager;
    public override IVoiceManager VoiceManager => _voiceManager;

    private IMatchmaker _matchmaker;
    public override IMatchmaker Matchmaker => _matchmaker;

    private bool _isServerActive;
    private bool _isConnectionActive;

    private EOSManager eosManager;
    private EOSAuthManager eosAuthManager;

    ProductUserId puid => eosAuthManager?.puid;

    public override bool CheckSupported()
    {
        return true;
    }

    public override bool CheckValidation()
    {
        return EOSSDKLoader.HasEOSSDK;
    }

    public override void OnInitializeLayer()
    {
        PlayerIDManager.SetPlatformID(puid.ToString());
        MelonCoroutines.Start(EOSUsernameDeterminer.GetUsernameAsync(s =>
        {
            LocalPlayer.Username = s;
        }));

        HookEOSEvents();

        _voiceManager = new UnityVoiceManager();
        _voiceManager.Enable();

        _matchmaker = new EOSMatchmaker();
    }

    public override void OnDeinitializeLayer()
    {
        Disconnect();

        _voiceManager.Disable();
        _voiceManager = null;

        _matchmaker = null;

        _currentLobby = null;

        eosManager.ShutdownEOS();

        UnHookEOSEvents();

        eosManager = null;
        eosAuthManager = null;
    }

    public override void LogIn()
    {
        eosAuthManager ??= new EOSAuthManager();

        eosManager ??= new EOSManager(eosAuthManager);

        NetworkLayerNotifications.SendLoggingInNotification();

        MelonCoroutines.Start(eosManager.InitializeAsync(success =>
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

    public override void OnUpdateLayer()
    {
        if (_isConnectionActive)
            EOSMessenger.ReceiveMessages();
    }

    public override string GetUsername(string platformId)
    {
        return EOSUsernameDeterminer.GetRandomUsername(platformId);
    }

    // Using device id basically gets rid of all epic account features. Including friends
    public override bool IsFriend(string userId)
    {
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
        var createOptions = new CreateLobbyOptions
        {
            BucketId = "FUSION",
            DisableHostMigration = true,
            LocalUserId = puid,
            MaxLobbyMembers = 64,
            PermissionLevel = LobbyPermissionLevel.Publicadvertised,
            EnableRTCRoom = false,
            PresenceEnabled = true,
            RejoinAfterKickRequiresInvite = false,
            EnableJoinById = true,
            AllowInvites = true,
        };
        
        EOSManager.LobbyInterface.CreateLobby(ref createOptions, null, ((ref CreateLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
                FusionLogger.Error($"Failed to create EOS lobby: {info.ResultCode}");
                return;
            }

            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = info.LobbyId,
                LocalUserId = puid,
            };
            EOSManager.LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);

            _isServerActive = true;
            _isConnectionActive = true;

            _currentLobby = new EpicLobby(lobbyDetails, info.LobbyId);

            InternalServerHelpers.OnStartServer();

            AddNotifyPeerEvents();

#if DEBUG
            FusionLogger.Log($"Created EOS lobby: {info.ResultCode} with ID {info.LobbyId}");
#endif

            RefreshServerCode();
        }));
    }

    // Notification IDs for handling different connection events
    private ulong connectionRequestedId = Common.INVALID_NOTIFICATIONID;
    private ulong connectionEstablishedId = Common.INVALID_NOTIFICATIONID;
    private ulong connectionClosedId = Common.INVALID_NOTIFICATIONID;
    private void AddNotifyPeerEvents()
    {
        if (IsHost)
        {
            var requestOptions = new AddNotifyPeerConnectionRequestOptions()
            {
                SocketId = EOSMessenger.SocketId,
                LocalUserId = puid
            };
            var closedOptions = new AddNotifyPeerConnectionClosedOptions
            {
                SocketId = EOSMessenger.SocketId,
                LocalUserId = puid
            };

            connectionRequestedId = EOSManager.P2PInterface.AddNotifyPeerConnectionRequest(ref requestOptions, null, (ref OnIncomingConnectionRequestInfo callbackInfo) =>
            {
                var acceptOptions = new AcceptConnectionOptions
                {
                    RemoteUserId = callbackInfo.RemoteUserId,
                    SocketId = EOSMessenger.SocketId,
                    LocalUserId = puid
                };
                EOSManager.P2PInterface.AcceptConnection(ref acceptOptions);
            });
            connectionClosedId = EOSManager.P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, (ref OnRemoteConnectionClosedInfo info) =>
            {
                var closeOptions = new CloseConnectionOptions
                {
                    RemoteUserId = info.RemoteUserId,
                    SocketId = EOSMessenger.SocketId,
                    LocalUserId = puid
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
                SocketId = EOSMessenger.SocketId,
                LocalUserId = puid
            };
            var closedOptions = new AddNotifyPeerConnectionClosedOptions
            {
                SocketId = EOSMessenger.SocketId,
                LocalUserId = puid
            };

            connectionEstablishedId = EOSManager.P2PInterface.AddNotifyPeerConnectionEstablished(ref establishedOptions, null, (ref OnPeerConnectionEstablishedInfo info) =>
            {
                ConnectionSender.SendConnectionRequest();
            });
            connectionClosedId = EOSManager.P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, (ref OnRemoteConnectionClosedInfo info) =>
            {
                // Disconnect when the host closes the lobby
                Disconnect();
            });
        }
    }

    private void RemoveNotification(ref ulong notificationId, Action<ulong> removeAction)
    {
        if (notificationId != Common.INVALID_NOTIFICATIONID)
        {
            removeAction(notificationId);
            notificationId = Common.INVALID_NOTIFICATIONID;
        }
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
   

    public void JoinServer(string lobbyId)
    {
        if (_isConnectionActive || _isServerActive)
            Disconnect();

        var joinLobbyOptions = new JoinLobbyByIdOptions
        {
            CrossplayOptOut = false,
            LobbyId = lobbyId,
            LocalUserId = puid,
            PresenceEnabled = true,
        };
        EOSManager.LobbyInterface.JoinLobbyById(ref joinLobbyOptions, null, (ref JoinLobbyByIdCallbackInfo joinDelegate) =>
        {
            if (joinDelegate.ResultCode != Result.Success)
            {
                FusionLogger.Log(lobbyId);
                FusionLogger.Error($"Failed to join EOS lobby: {joinDelegate.ResultCode}");
                return;
            }

            CopyLobbyDetailsHandleOptions copyLobbyDetailsHandleOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = joinDelegate.LobbyId,
                LocalUserId = puid
            };
            EOSManager.LobbyInterface.CopyLobbyDetailsHandle(ref copyLobbyDetailsHandleOptions, out var lobbyDetails);

            LobbyDetailsGetLobbyOwnerOptions lobbyDetailsGetLobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId hostId = lobbyDetails.GetLobbyOwner(ref lobbyDetailsGetLobbyOwnerOptions);

            _isServerActive = false;
            _isConnectionActive = true;

            _currentLobby = new EpicLobby(lobbyDetails, joinDelegate.LobbyId);

            // Add events so once we have a connection, we join on the fusion end
            AddNotifyPeerEvents();

            // Send a dummy packet to establish the connection
            NetMessage message = NetMessage.Create(0, Array.Empty<byte>(), CommonMessageRoutes.None);
            EOSMessenger.SendPacket(hostId, message, NetworkChannel.Reliable, false);
        });
    }

    public override void Disconnect(string reason = "")
    {
        if (!_isServerActive && !_isConnectionActive)
            return;

        if (IsHost)
            DestroyLobby(reason);
        else
            LeaveLobby(reason);
    }
    
    private void OnDisconnectComplete(string reason)
    {
        RemoveNotifyPeerEvents();
        
        CloseConnectionsOptions closeConnectionsOptions = new CloseConnectionsOptions
        {
            LocalUserId = puid,
            SocketId = EOSMessenger.SocketId,
        };

        EOSManager.P2PInterface.CloseConnections(ref closeConnectionsOptions);

        _isServerActive = false;
        _isConnectionActive = false;

        _currentLobby = null;
        _serverCode = string.Empty;

        InternalServerHelpers.OnDisconnect(reason);
    }
    
    private void DestroyLobby(string reason)
    {
        var destroyOptions = new DestroyLobbyOptions
        {
            LocalUserId = puid,
            LobbyId = _currentLobby.LobbyId
        };

        EOSManager.LobbyInterface.DestroyLobby(ref destroyOptions, null, (ref DestroyLobbyCallbackInfo info) => OnDisconnectComplete(reason));
    }

    private void LeaveLobby(string reason)
    {
        var leaveOptions = new LeaveLobbyOptions
        {
            LocalUserId = puid,
            LobbyId = _currentLobby.LobbyId
        };

        EOSManager.LobbyInterface.LeaveLobby(ref leaveOptions, null, (ref LeaveLobbyCallbackInfo info) => OnDisconnectComplete(reason));
    }


    public override void DisconnectUser(string platformID)
    {
        // Make sure we are hosting a server
        if (!_isServerActive)
        {
            return;
        }

        KickMemberOptions kickMemberOptions = new KickMemberOptions()
        {
            LobbyId = _currentLobby.LobbyId,
            LocalUserId = puid,
            TargetUserId = ProductUserId.FromString(platformID),
        };
        
        EOSManager.LobbyInterface.KickMember(ref kickMemberOptions, null, (ref KickMemberCallbackInfo data) =>
        {
            
        });
    }

    private string _serverCode;

    public override string GetServerCode()
    {
        return _serverCode;
    }

    public override void RefreshServerCode()
    {
        _serverCode = RandomCodeGenerator.GetString(8);

        LobbyInfoManager.PushLobbyUpdate();
    }

    public override string GetServerID() => _currentLobby.LobbyId;

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
                FusionLogger.Log("No lobbies found with the given code.");
                return;
            }

            JoinServer(info.Lobbies[0].Metadata.LobbyInfo.LobbyID);
        });
    }

    private void HookEOSEvents()
    {
        // Add server hooks
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
        MultiplayerHooking.OnDisconnected += OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;
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

    private void OnUpdateLobby()
    {
        // Make sure the lobby exists
        if (Lobby == null)
        {
#if DEBUG
            FusionLogger.Warn("Tried updating the EOS lobby, but it was null!");
#endif
            return;
        }

        // Write active info about the lobby
        LobbyMetadataSerializer.WriteInfo(Lobby);
    }

    private void UnHookEOSEvents()
    {
        // Remove server hooks
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeave;
        MultiplayerHooking.OnDisconnected -= OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged -= OnUpdateLobby;
    }
}