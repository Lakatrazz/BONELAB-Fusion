using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;

using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Voice;
using LabFusion.Voice.Unity;

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
	public override string Title => "Epic Online Services";

	public override string Platform => "Epic";

	public override bool RequiresValidId => false;

	public override bool ServerCanSendToHost => false;

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
	protected bool _isInitialized = false;

	internal static LogLevel LogLevel => LogLevel.Warning;

	// EOS Interfaces
	internal static PlatformInterface PlatformInterface;
	internal static AuthInterface AuthInterface;
	internal static ConnectInterface ConnectInterface;
	internal static P2PInterface P2PInterface;
	internal static LobbyInterface LobbyInterface;
	internal static FriendsInterface FriendsInterface;

	internal LobbyDetails LobbyDetails => _currentLobby?.LobbyDetails;

    public LobbyConnectionState LobbyConnectionState { get; private set; } = LobbyConnectionState.Disconnected;

	public static ProductUserId LocalUserId;
	public static EpicAccountId LocalAccountId;

	internal static ProductUserId HostId = null;

	public override bool CheckSupported() => true;

	public override bool CheckValidation() => EOSSDKLoader.HasEOSSDK;

	public override void OnInitializeLayer()
	{
		try
		{
            EOSManager.InitEOS();

            _voiceManager = new UnityVoiceManager();
			_voiceManager.Enable();

			_matchmaker = new EOSMatchmaker(LobbyInterface);

			HookEOSEvents();

            _isInitialized = true;
		}
		catch (Exception e)
		{
			FusionLogger.Error($"Failed to initialize EOS Network Layer: {e.Message}");
			_isInitialized = false;
			return;
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

        UnhookEOSEvents();

        _isInitialized = false;
	}

	private void HookEOSEvents()
	{
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
        MultiplayerHooking.OnDisconnected += OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;
    }

	private void UnhookEOSEvents()
	{
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeave;
        MultiplayerHooking.OnDisconnected -= OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged -= OnUpdateLobby;
    }

	public override void LogIn()
	{
		InvokeLoggedInEvent();
	}

	public override void LogOut()
	{
		InvokeLoggedOutEvent();
	}

    public override void OnUpdateLayer()
	{
        PlatformInterface?.Tick();

		// Used for throttling metadata updates since EOS has a limit
		if (IsHost)
			_currentLobby?.UpdateLobby();

		EOSSocketHandler.ReceiveMessages();
	}

	public void OnUpdateLobby()
	{
		if (_currentLobby == null)
		{
			return;
		}

		LobbyMetadataSerializer.WriteInfo(_currentLobby);

		if (_currentLobby == null)
			return;

		var copyOptions = new CopyLobbyDetailsHandleOptions
		{
			LobbyId = _currentLobby.LobbyId,
			LocalUserId = LocalUserId
		};

		Result result = LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);

		_currentLobby.UpdateLobbyDetails(lobbyDetails);
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

	public override string GetUsername(string userId)
	{
		string username = "Unknown";

		if (string.IsNullOrEmpty(userId))
		{
			FusionLogger.Error("GetUsername called with null or empty userId.");
			return username;
		}

		EpicAccountId epicAccountId = EpicAccountId.FromString(userId);

		var userInfoInterface = PlatformInterface.GetUserInfoInterface();
		if (userInfoInterface != null)
		{
			var userInfoOptions = new Epic.OnlineServices.UserInfo.QueryUserInfoOptions
			{
				LocalUserId = LocalAccountId,
				TargetUserId = epicAccountId
			};

			userInfoInterface.QueryUserInfo(ref userInfoOptions, null, (ref Epic.OnlineServices.UserInfo.QueryUserInfoCallbackInfo callbackInfo) =>
			{
				if (callbackInfo.ResultCode == Result.Success)
				{
					var copyOptions = new Epic.OnlineServices.UserInfo.CopyUserInfoOptions
					{
						LocalUserId = LocalAccountId,
						TargetUserId = epicAccountId
					};

					if (userInfoInterface.CopyUserInfo(ref copyOptions, out var userInfo) == Result.Success)
					{
						// This is done Asynchronously, so we cant just return the username. Instead we can just set LocalPlayer.Username as this method is only used for the local player. Fakin hell
						LocalPlayer.Username = userInfo?.DisplayName ?? "Unknown User";
					}
				}
			});

			return username;
		}

		return username;
	}

    public override bool IsFriend(string Id)
    {
        // Fusion calls this with the id of the lobby, we need to convert the lobby id into the host id
        ProductUserId productUserId = ProductUserId.FromString(Id);
        EpicAccountId epicAccountId = EOSManager.GetEpicAccountIdFromProductUserId(productUserId);

        // this must be a lobby id if we failed to get the epic account id
        if (epicAccountId == null)
        {
            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = Id,
                LocalUserId = LocalUserId
            };
            LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);

            var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            epicAccountId = EOSManager.GetEpicAccountIdFromProductUserId(lobbyDetails.GetLobbyOwner(ref ownerOptions));
            lobbyDetails.Release();
        }

        if (LocalAccountId == epicAccountId)
            return true;

        var friendsInterface = PlatformInterface.GetFriendsInterface();

        var statusOptions = new Epic.OnlineServices.Friends.GetStatusOptions()
        {
            LocalUserId = LocalAccountId,
            TargetUserId = epicAccountId
        };
        var friendStatus = friendsInterface.GetStatus(ref statusOptions);

        return friendStatus == Epic.OnlineServices.Friends.FriendsStatus.Friends;
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
		if (!_isInitialized)
			return;

		if (LobbyConnectionState == LobbyConnectionState.Connecting)
			return;

		SetLobbyConnectionState(LobbyConnectionState.Connecting);
		CreateEpicLobby();
	}

	private ulong _connectionRequestedId = Common.InvalidNotificationid;
	private ulong _connectionClosedId = Common.InvalidNotificationid;
    private void CreateEpicLobby()
	{
		var createOptions = new CreateLobbyOptions
		{
			BucketId = "BONELABFUSION",
			DisableHostMigration = true,
			LocalUserId = LocalUserId,
			MaxLobbyMembers = 64,
			PermissionLevel = LobbyPermissionLevel.Publicadvertised,
			EnableRTCRoom = false,
			PresenceEnabled = false,
			RejoinAfterKickRequiresInvite = false,
			EnableJoinById = true,
		};
		LobbyInterface.CreateLobby(ref createOptions, null, (ref CreateLobbyCallbackInfo info) =>
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
			LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);

			_currentLobby = new EOSLobby(lobbyDetails, info.LobbyId);

			var requestOptions = new AddNotifyPeerConnectionRequestOptions
			{
				SocketId = EOSSocketHandler.SocketId,
				LocalUserId = LocalUserId
			};
			_connectionRequestedId = P2PInterface.AddNotifyPeerConnectionRequest(ref requestOptions, null, (ref OnIncomingConnectionRequestInfo callbackInfo) =>
			{
				var acceptOptions = new AcceptConnectionOptions
				{
					RemoteUserId = callbackInfo.RemoteUserId,
					SocketId = EOSSocketHandler.SocketId,
					LocalUserId = LocalUserId
				};
				P2PInterface.AcceptConnection(ref acceptOptions);
			});

			var closedOptions = new AddNotifyPeerConnectionClosedOptions
			{
				SocketId = EOSSocketHandler.SocketId,
				LocalUserId = LocalUserId
			};
			_connectionClosedId = P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null , (ref OnRemoteConnectionClosedInfo info) =>
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
				P2PInterface.CloseConnection(ref closeOptions);
			});

			_isServerActive = true;
			_isConnectionActive = false;

			HostId = LocalUserId;

			InternalServerHelpers.OnStartServer();

#if DEBUG
			FusionLogger.Log($"Created EOS lobby: {info.ResultCode} with ID {info.LobbyId}");
#endif

			SetLobbyConnectionState(LobbyConnectionState.Connected);
			RefreshServerCode();
		});
	}

	public override void Disconnect(string reason = "")
	{
		if (!_isServerActive && !_isConnectionActive)
			return;

		SetLobbyConnectionState(LobbyConnectionState.Disconnecting);

        string lobbyId = _currentLobby.LobbyId;

		if (string.IsNullOrEmpty(lobbyId))
			return;

        if (IsHost)
        {
            var destroyOptions = new DestroyLobbyOptions
            {
                LocalUserId = LocalUserId,
                LobbyId = lobbyId
            };

            LobbyInterface.DestroyLobby(ref destroyOptions, null, (ref DestroyLobbyCallbackInfo info) =>
            {
				P2PInterface.RemoveNotifyPeerConnectionRequest(_connectionRequestedId);
				P2PInterface.RemoveNotifyPeerConnectionClosed(_connectionClosedId);

                EOSSocketHandler.CloseConnections();
                InternalServerHelpers.OnDisconnect(reason);

                _isServerActive = false;
                _isConnectionActive = false;
                _currentLobby = null;

                SetLobbyConnectionState(LobbyConnectionState.Disconnected);
            });
        }
		else
		{
            var leaveOptions = new LeaveLobbyOptions
            {
                LobbyId = lobbyId,
                LocalUserId = LocalUserId
            };

            LobbyInterface.LeaveLobby(ref leaveOptions, null, (ref LeaveLobbyCallbackInfo info) =>
            {
                EOSSocketHandler.CloseConnections();
                InternalServerHelpers.OnDisconnect(reason);

                _isServerActive = false;
				_isConnectionActive = false;
                _currentLobby = null;

                SetLobbyConnectionState(LobbyConnectionState.Disconnected);
            });
        }
	}

	private void OnDisconnect()
	{
		if (VoiceManager == null)
		{
			return;
		}

		VoiceManager.ClearManager();
	}

	public void JoinServer(string lobbyId)
	{
		if (LobbyConnectionState == LobbyConnectionState.Connecting)
			return;

		if (_isConnectionActive || _isServerActive)
			Disconnect();

		if (!_isInitialized)
		{
			FusionLogger.Error("Cannot join server: EOS not initialized");
			return;
		}

		SetLobbyConnectionState(LobbyConnectionState.Connecting);

		var joinLobbyOptions = new JoinLobbyByIdOptions
		{
			CrossplayOptOut = false,
			LobbyId = lobbyId,
			LocalUserId = LocalUserId,
			PresenceEnabled = false,
		};
		LobbyInterface.JoinLobbyById(ref joinLobbyOptions, null, (ref JoinLobbyByIdCallbackInfo joinDelegate) =>
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
			LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);
			var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();

			HostId = lobbyDetails.GetLobbyOwner(ref ownerOptions);

			_isServerActive = false;
			_isConnectionActive = true;

			ConnectionSender.SendConnectionRequest();
			FusionLogger.Log($"Joined EOS lobby: {joinDelegate.ResultCode} with owner {HostId}");
			_currentLobby = new EOSLobby(lobbyDetails, joinDelegate.LobbyId);
			SetLobbyConnectionState(LobbyConnectionState.Connected);
		});
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
				FusionLogger.Log("No lobbies found with the given code.");	
				return;
			}

			JoinServer(info.Lobbies[0].Metadata.LobbyInfo.LobbyId);
		});
	}

	public override void BroadcastMessage(NetworkChannel channel, NetMessage message)
	{
		if (IsHost)
		{
			EOSSocketHandler.BroadcastToClients(channel, message);
		}
		else
		{
			EOSSocketHandler.BroadcastToServer(channel, message);
		}
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