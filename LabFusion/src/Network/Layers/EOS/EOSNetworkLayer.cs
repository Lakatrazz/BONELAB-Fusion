using Epic.OnlineServices;
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
using static Il2CppSystem.Globalization.CultureInfo;

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
	protected bool _isInitialized = false;

	internal static LogLevel LogLevel => LogLevel.Warning;

	internal LobbyDetails LobbyDetails => _currentLobby?.LobbyDetails;

	public LobbyConnectionState LobbyConnectionState { get; private set; } = LobbyConnectionState.Disconnected;

	public static ProductUserId LocalUserId;
	public static EpicAccountId LocalAccountId;

	internal static ProductUserId HostId = null;

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
			{
				InvokeLoggedInEvent();
			} else
			{
				FusionLogger.Error("Failed to log in to EOS Network Layer.");
			}
		}));
	}

	public override void LogOut()
	{
		InvokeLoggedOutEvent();
	}

	public override string GetUsername(string userId)
	{
		return EOSManager.SavedUsername;
	}

	public override void OnInitializeLayer()
	{
		try
		{
			PlayerIDManager.SetStringID(LocalUserId.ToString());
			LocalPlayer.Username = GetUsername(EOSManager.SavedUsername);

			_voiceManager = new UnityVoiceManager();
			_voiceManager.Enable();

			_matchmaker = new EOSMatchmaker(EOSManager.LobbyInterface);

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

	public override void OnUpdateLayer()
	{
		EOSSocketHandler.ReceiveMessages();
	}

	public void OnUpdateLobby()
	{
		if (_currentLobby == null)
		{
			return;
		}

		LobbyMetadataSerializer.WriteInfo(Lobby);

		if (_currentLobby == null)
			return;

		var copyOptions = new CopyLobbyDetailsHandleOptions
		{
			LobbyId = _currentLobby.LobbyId,
			LocalUserId = LocalUserId
		};

		Result result = EOSManager.LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);

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

	// Fusion calls this with the id of the lobby, we need to convert the lobby id into the host id
	public override bool IsFriend(string Id)
	{
		ProductUserId productUserId = ProductUserId.FromString(Id);
		EpicAccountId epicAccountId = EOSUtils.GetAccountIdFromProductId(productUserId);

		// this must be a lobby id if we failed to get the epic account id
		if (epicAccountId == null)
		{
			var copyOptions = new CopyLobbyDetailsHandleOptions
			{
				LobbyId = Id,
				LocalUserId = LocalUserId
			};
			EOSManager.LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);

			var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
			epicAccountId = EOSUtils.GetAccountIdFromProductId(lobbyDetails.GetLobbyOwner(ref ownerOptions));
			lobbyDetails.Release();
		}

		if (LocalAccountId == epicAccountId)
			return true;

		var statusOptions = new Epic.OnlineServices.Friends.GetStatusOptions()
		{
			LocalUserId = LocalAccountId,
			TargetUserId = epicAccountId
		};
		var friendStatus = EOSManager.FriendsInterface.GetStatus(ref statusOptions);

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

			_currentLobby = new EOSLobby(lobbyDetails, info.LobbyId);

			var requestOptions = new AddNotifyPeerConnectionRequestOptions
			{
				SocketId = EOSSocketHandler.SocketId,
				LocalUserId = LocalUserId
			};
			_connectionRequestedId = EOSManager.P2PInterface.AddNotifyPeerConnectionRequest(ref requestOptions, null, (ref OnIncomingConnectionRequestInfo callbackInfo) =>
			{
				var acceptOptions = new AcceptConnectionOptions
				{
					RemoteUserId = callbackInfo.RemoteUserId,
					SocketId = EOSSocketHandler.SocketId,
					LocalUserId = LocalUserId
				};
				EOSManager.P2PInterface.AcceptConnection(ref acceptOptions);
			});

			var closedOptions = new AddNotifyPeerConnectionClosedOptions
			{
				SocketId = EOSSocketHandler.SocketId,
				LocalUserId = LocalUserId
			};
			_connectionClosedId = EOSManager.P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, (ref OnRemoteConnectionClosedInfo info) =>
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

			EOSManager.LobbyInterface.DestroyLobby(ref destroyOptions, null, (ref DestroyLobbyCallbackInfo info) =>
			{
				EOSManager.P2PInterface.RemoveNotifyPeerConnectionRequest(_connectionRequestedId);
				EOSManager.P2PInterface.RemoveNotifyPeerConnectionClosed(_connectionClosedId);

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

			EOSManager.LobbyInterface.LeaveLobby(ref leaveOptions, null, (ref LeaveLobbyCallbackInfo info) =>
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

		SetLobbyConnectionState(LobbyConnectionState.Connecting);

		var joinLobbyOptions = new JoinLobbyByIdOptions
		{
			CrossplayOptOut = false,
			LobbyId = lobbyId,
			LocalUserId = LocalUserId,
			PresenceEnabled = false,
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
			
			Epic.OnlineServices.P2P.AddNotifyPeerConnectionEstablishedOptions connectionOptions = new Epic.OnlineServices.P2P.AddNotifyPeerConnectionEstablishedOptions
			{
				SocketId = EOSSocketHandler.SocketId,
				LocalUserId = LocalUserId
			};

            ulong connectionEstablishedId = Common.InvalidNotificationid;
            connectionEstablishedId = EOSManager.P2PInterface.AddNotifyPeerConnectionEstablished(ref connectionOptions, null, (ref OnPeerConnectionEstablishedInfo data) =>
			{
				EOSManager.P2PInterface.RemoveNotifyPeerConnectionEstablished(connectionEstablishedId);
                ConnectionSender.SendConnectionRequest();

                SetLobbyConnectionState(LobbyConnectionState.Connected);
            });

            // Send a dummy packet to establish the connection
            EOSSocketHandler.SendPacketToUser(HostId, Array.Empty<byte>(), NetworkChannel.Reliable, false);
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