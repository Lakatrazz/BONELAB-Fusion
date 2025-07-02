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
using UnityEngine;
using static LiteNetLib.EventBasedNetListener;

namespace LabFusion.Network;

public class EOSNetworkLayer : NetworkLayer
{
	public const int ReceiveBufferSize = 32;

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

	public override string GetUsername(string userId) => string.Empty;

	public override bool IsFriend(string userId)
	{
		return userId == PlayerIDManager.LocalPlatformID;
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

	public override void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
	{
		var playerID = PlayerIDManager.GetPlayerID(userId);
		if (playerID != null && !string.IsNullOrEmpty(playerID.PlatformID))
		{
			SendFromServer(playerID.PlatformID, channel, message);
		}
	}

	public override void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
	{
		EOSSocketHandler.SendFromServer(userId, channel, message);
	}

	public override void StartServer()
	{
		if (!_isInitialized)
		{
			return;
		}

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

            // Done creating lobby, now start the server
			_isServerActive = true;
			_isConnectionActive = false;

			HostId = LocalUserId;

            _currentLobby.SetMetadata("lobby_open", bool.TrueString);

            OnUpdateLobby();

            InternalServerHelpers.OnStartServer();
            RefreshServerCode();
        });
    }

    public override void Disconnect(string reason = "")
	{
		if (!_isServerActive && !_isConnectionActive)
			return;

		if (_currentLobby != null && PlatformInterface != null)
		{
			if (LobbyInterface != null && LocalUserId != null)
			{
				string lobbyId = (_currentLobby as EOSLobby)?.GetLobbyId();
				if (!string.IsNullOrEmpty(lobbyId))
				{
					var leaveOptions = new Epic.OnlineServices.Lobby.LeaveLobbyOptions
					{
						LobbyId = lobbyId,
						LocalUserId = LocalUserId
					};

					LobbyInterface.LeaveLobby(ref leaveOptions, null, (ref Epic.OnlineServices.Lobby.LeaveLobbyCallbackInfo info) =>
					{
						FusionLogger.Log($"Left EOS lobby: {info.ResultCode}");

						_isServerActive = false;
						_isConnectionActive = false;

						InternalServerHelpers.OnDisconnect(reason);
					});
				}
			}

			_currentLobby = null;
		}
	}

	public void JoinServer(string lobbyId)
	{
		if (_isConnectionActive || _isServerActive)
			Disconnect();

		if (!_isInitialized)
		{
			FusionLogger.Error("Cannot join server: EOS not initialized");
			return;
		}

		var joinLobbyOptions = new JoinLobbyByIdOptions
		{
			CrossplayOptOut = false,
			LobbyId = lobbyId,
			LocalUserId = LocalUserId,
			PresenceEnabled = false,
        };
		LobbyInterface.JoinLobbyById(ref joinLobbyOptions, null, (ref JoinLobbyByIdCallbackInfo joinDelegate) =>
		{
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
        });
	}

	public void OnUpdateLobby()
	{
		if (_currentLobby == null)
		{
			return;
		}

		LobbyMetadataHelper.WriteInfo(_currentLobby);

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
}