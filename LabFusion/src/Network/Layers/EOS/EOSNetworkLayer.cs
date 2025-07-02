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

	public override bool RequiresValidId => true;

	public override bool IsHost => _isServerActive;
	public override bool IsClient => _isConnectionActive;

	private INetworkLobby _currentLobby;
	public override INetworkLobby Lobby => _currentLobby;

	private IVoiceManager _voiceManager = null;
	public override IVoiceManager VoiceManager => _voiceManager;

	private IMatchmaker _matchmaker = null;
	public override IMatchmaker Matchmaker => _matchmaker;

	protected bool _isServerActive = false;
	protected bool _isConnectionActive = false;
	protected bool _isInitialized = false;

	internal static LogLevel LogLevel => LogLevel.Verbose;

    // EOS Interfaces
    internal static PlatformInterface PlatformInterface;
	internal static AuthInterface AuthInterface;
    internal static ConnectInterface ConnectInterface;
    internal static P2PInterface P2PInterface;

	internal static LobbyDetails LobbyDetails;

    public static ProductUserId LocalUserId;
    public static EpicAccountId LocalAccountId;

	internal static ProductUserId HostId = null;

    private const float _PlatformTickInterval = 0.1f;
	private float _PlatformTickTimer = 0f;

	public string ServerCode { get; private set; } = null;

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
		try
		{
			EOSAuthenticator.InitializeEOS();

			_voiceManager = new UnityVoiceManager();
			_voiceManager.Enable();

			_matchmaker = new EOSMatchmaker(PlatformInterface.GetLobbyInterface());

			P2PInterface = PlatformInterface.GetP2PInterface();

			MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
			MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
			MultiplayerHooking.OnDisconnected += OnDisconnect;

			LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;

			_isInitialized = true;
		}
		catch (Exception e)
		{
			FusionLogger.LogException("Failed to initialize EOS layer", e);
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
		LobbyDetails = null;

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
		EOSLobby Lobby = _currentLobby as EOSLobby;
		Lobby?.UpdateLobby();

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

		CreateLobby();

		_isServerActive = true;
		_isConnectionActive = true;
	}

	public override void Disconnect(string reason = "")
	{
		if (!_isServerActive && !_isConnectionActive)
			return;

		if (_currentLobby != null && PlatformInterface != null)
		{
			var lobbyInterface = PlatformInterface.GetLobbyInterface();
			if (lobbyInterface != null && LocalUserId != null)
			{
				string lobbyId = (_currentLobby as EOSLobby)?.GetLobbyId();
				if (!string.IsNullOrEmpty(lobbyId))
				{
					var leaveOptions = new Epic.OnlineServices.Lobby.LeaveLobbyOptions
					{
						LobbyId = lobbyId,
						LocalUserId = LocalUserId
					};

					lobbyInterface.LeaveLobby(ref leaveOptions, null, (ref Epic.OnlineServices.Lobby.LeaveLobbyCallbackInfo info) =>
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

	public override string GetServerCode()
	{
		return ServerCode;
	}

	public override void RefreshServerCode()
	{
		ServerCode = RandomCodeGenerator.GetString(8);
	}

	public override void JoinServerByCode(string code)
	{
		if (Matchmaker == null)
		{
			return;
		}

		FusionLogger.Log($"Searching for EOS servers with code {code}...");

		Matchmaker.RequestLobbies((info) =>
		{
			foreach (var lobbyInfo in info.Lobbies)
			{
				if (lobbyInfo.Metadata.LobbyInfo.LobbyCode.ToLower() == code.ToLower())
				{
					ProductUserId hostId = null;
					if (lobbyInfo.Lobby is EOSLobby eosLobby)
					{
						var hostIdStr = eosLobby.GetMetadata("host_id");
						if (!string.IsNullOrEmpty(hostIdStr))
						{
							hostId = ProductUserId.FromString(hostIdStr);
						}
					}

					if (hostId != null)
					{
						JoinServer(hostId);
					}
					break;
				}
			}
		});
	}

	public void JoinServer(ProductUserId hostId)
	{
		if (_isConnectionActive || _isServerActive)
			Disconnect();

		if (!_isInitialized)
		{
			FusionLogger.Error("Cannot join server: EOS not initialized");
			return;
		}

		HostId = hostId;

        _isServerActive = false;
		_isConnectionActive = true;

		ConnectionSender.SendConnectionRequest();
	}

	public void OnUpdateLobby()
	{
		if (_currentLobby == null)
		{
			return;
		}

		LobbyMetadataHelper.WriteInfo(_currentLobby);
	}

	private void CreateLobby()
	{
        var lobbyInterface = PlatformInterface.GetLobbyInterface();

		var createOptions = new CreateLobbyOptions
		{
			LocalUserId = LocalUserId,
			MaxLobbyMembers = 64,
			PermissionLevel = LobbyPermissionLevel.Publicadvertised,
			BucketId = "BONELABFUSION",
			EnableRTCRoom = false,
			DisableHostMigration = true,
			AllowInvites = true,
			PresenceEnabled = true,
        };

		lobbyInterface.CreateLobby(ref createOptions, null, (ref CreateLobbyCallbackInfo info) =>
		{
			if (info.ResultCode == Result.Success)
			{
#if DEBUG
				FusionLogger.Log($"EOS Lobby created successfully with ID: {info.LobbyId}");
#endif

				var copyOptions = new CopyLobbyDetailsHandleOptions
				{
					LobbyId = info.LobbyId,
					LocalUserId = LocalUserId
				};

				Result result = lobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);
				if (result == Result.Success && lobbyDetails != null)
				{
					_currentLobby = new EOSLobby(lobbyDetails, info.LobbyId);
					EOSLobby lobby = _currentLobby as EOSLobby;
                    LobbyDetails = lobby.LobbyDetails;

                    _currentLobby.SetMetadata("lobby_open", bool.TrueString);
                    _currentLobby.SetMetadata("host_id", LocalUserId.ToString());
					_currentLobby.SetMetadata("server_name", $"Fusion Server by {LocalPlayer.Username}");

                    if (ServerCode != null)
					{
						_currentLobby.SetMetadata("lobby_code", ServerCode);
					}

					_currentLobby.WriteKeyCollection();

					HostId = LocalUserId;

					EOSSocketHandler.ConfigureP2PSocketToAcceptConnections();

					var options = new AddNotifyPeerConnectionRequestOptions()
					{
						LocalUserId = LocalUserId,
						SocketId = EOSSocketHandler.SocketId
					};

                    P2PInterface.AddNotifyPeerConnectionRequest(ref options, null, (ref OnIncomingConnectionRequestInfo data) =>
                    {
                        FusionLogger.Log("Incoming P2P connection request...");
                        var acceptOptions = new AcceptConnectionOptions
                        {
                            LocalUserId = LocalUserId,
                            RemoteUserId = data.RemoteUserId,
                            SocketId = data.SocketId
                        };
                        P2PInterface.AcceptConnection(ref acceptOptions);
                        FusionLogger.Log("Accepted connection from client.");
                    });

                    InternalServerHelpers.OnStartServer();

                    RefreshServerCode();
                }
				else
				{
					FusionLogger.Error($"Failed to get lobby details: {result}");
				}
			}
			else
			{
				FusionLogger.Error($"Failed to create EOS lobby: {info.ResultCode}");
			}
		});
	}
}