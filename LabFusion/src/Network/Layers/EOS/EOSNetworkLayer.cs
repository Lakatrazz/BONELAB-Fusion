using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
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

	internal LobbyDetails LobbyDetails => _currentLobby?.LobbyDetails;
	public LobbyConnectionState LobbyConnectionState { get; private set; } = LobbyConnectionState.Disconnected;

	public static ProductUserId LocalUserId;
	public static EpicAccountId LocalAccountId;

	internal static ProductUserId HostId = null;

	private const float _PlatformTickInterval = 0.1f;
	private float _PlatformTickTimer = 0f;

	public string ServerCode { get; private set; } = null;

	public override bool CheckSupported() => true;

	public override bool CheckValidation() => EOSSDKLoader.HasEOSSDK;

	public override void OnInitializeLayer()
	{
		try
		{
			EOSAuthenticator.InitializeEOS();

			_voiceManager = new UnityVoiceManager();
			_voiceManager.Enable();

			LobbyInterface = PlatformInterface.GetLobbyInterface();

			_matchmaker = new EOSMatchmaker(LobbyInterface);

			P2PInterface = PlatformInterface.GetP2PInterface();

			MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
			MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
			MultiplayerHooking.OnDisconnected += OnDisconnect;

			LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;

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
		MultiplayerHooking.OnPlayerJoined -= OnPlayerJoin;
		MultiplayerHooking.OnPlayerLeft -= OnPlayerLeave;
		MultiplayerHooking.OnDisconnected -= OnDisconnect;

		LobbyInfoManager.OnLobbyInfoChanged -= OnUpdateLobby;

		_voiceManager?.Disable();
		_voiceManager = null;

		_matchmaker = null;

		_currentLobby = null;

		Disconnect();
		SetLobbyConnectionState(LobbyConnectionState.Disconnected);

		if (PlatformInterface != null)
		{
			PlatformInterface.Release();
			PlatformInterface = null;
		}

		_isInitialized = false;
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
		if (PlatformInterface != null)
		{
			_PlatformTickTimer += TimeUtilities.DeltaTime;

			if (_PlatformTickTimer >= _PlatformTickInterval)
			{
				_PlatformTickTimer = 0;
				PlatformInterface.Tick();
			}
		}

		// Used for throttling metadata updates since EOS has a limit
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
			LobbyId = _currentLobby.GetLobbyId(),
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

	public override bool IsFriend(string userId)
	{
		if (string.IsNullOrEmpty(userId) || LocalAccountId == null)
			return false;

		if (LocalUserId.ToString() == userId)
			return true;

		ProductUserId productUserId = ProductUserId.FromString(userId);
		EpicAccountId epicAccountId = EOSAuthenticator.GetEpicAccountIdFromProductUserId(productUserId);
		var friendsInterface = PlatformInterface.GetFriendsInterface();
		if (friendsInterface == null)
		{
			FusionLogger.Error("Friends interface is null, cannot check if user is a friend.");
			return false;
		}

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
			LobbyId = LocalUserId.ToString(),
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
			P2PInterface.AddNotifyPeerConnectionRequest(ref requestOptions, null, (ref OnIncomingConnectionRequestInfo callbackInfo) =>
			{
				OnUpdateLobby();
				var acceptOptions = new AcceptConnectionOptions
				{
					RemoteUserId = callbackInfo.RemoteUserId,
					SocketId = EOSSocketHandler.SocketId,
					LocalUserId = LocalUserId
				};
				P2PInterface.AcceptConnection(ref acceptOptions);
				OnUpdateLobby();
			});
			var closedOptions = new AddNotifyPeerConnectionClosedOptions
			{
				SocketId = EOSSocketHandler.SocketId,
				LocalUserId = LocalUserId
			};
			P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null , (ref OnRemoteConnectionClosedInfo info) =>
			{
				OnUpdateLobby();
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
				OnUpdateLobby();
			});

			_isServerActive = true;
			_isConnectionActive = false;

			HostId = LocalUserId;

			_currentLobby.SetMetadata("lobby_open", bool.TrueString);

			OnUpdateLobby();

			InternalServerHelpers.OnStartServer();
			FusionLogger.Log($"Created EOS lobby: {info.ResultCode} with ID {info.LobbyId}");
			SetLobbyConnectionState(LobbyConnectionState.Connected);
			RefreshServerCode();
		});
	}

	public override void Disconnect(string reason = "")
	{
		if (!_isServerActive && !_isConnectionActive)
			return;

		bool destroyingLobby = IsHost && _currentLobby != null && LobbyInterface != null && LocalUserId != null;
		bool leavingLobby = !IsHost && _currentLobby != null && LobbyInterface != null && LocalUserId != null;

		FusionLogger.Warn($"Disconnecting from EOS lobby: {reason} (Destroying: {destroyingLobby}, Leaving: {leavingLobby})");

		SetLobbyConnectionState(LobbyConnectionState.Disconnecting);

		if (destroyingLobby)
		{
			string lobbyId = _currentLobby.GetLobbyId();
			if (!string.IsNullOrEmpty(lobbyId))
			{
				var destroyOptions = new DestroyLobbyOptions
				{
					LocalUserId = LocalUserId,
					LobbyId = lobbyId
				};

				LobbyInterface.DestroyLobby(ref destroyOptions, null, (ref DestroyLobbyCallbackInfo info) =>
				{
					InternalServerHelpers.OnDisconnect(reason);
					EOSSocketHandler.CloseAllConnections();
					SetLobbyConnectionState(LobbyConnectionState.Disconnected);
				});
			}
		}

		else if (leavingLobby)
		{
			string lobbyId = _currentLobby.GetLobbyId();
			if (!string.IsNullOrEmpty(lobbyId))
			{
				var leaveOptions = new LeaveLobbyOptions
				{
					LobbyId = lobbyId,
					LocalUserId = LocalUserId
				};

				LobbyInterface.LeaveLobby(ref leaveOptions, null, (ref LeaveLobbyCallbackInfo info) =>
				{
					InternalServerHelpers.OnDisconnect(reason);
					EOSSocketHandler.CloseAllConnections();
					SetLobbyConnectionState(LobbyConnectionState.Disconnected);
				});
			}
		}

		_isServerActive = false;
		_isConnectionActive = false;

		_currentLobby = null;
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