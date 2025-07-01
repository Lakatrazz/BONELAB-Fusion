using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
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

	// EOS Properties
	string ProductName => "BONELAB Fusion";
	string ProductVersion => "1.0";
	string ProductId => "29e074d5b4724f3bb01f26b7e33d2582";
	string SandboxId => "26f32d66d87f4dfeb4a7449b776a41f1";
	string DeploymentId => "1dffb21201e04ad89b0e6e415f0b8993";
	string ClientId => "xyza7891gWLwVJx3rdLOLs6vJ05u9jWT";
	string ClientSecret => "IWrUy1Z62wWajAX37k3zkQ4Kkto+AvfQSyZ9zfvibzw";

	LogLevel logLevel = LogLevel.VeryVerbose;

	protected bool _isServerActive = false;
	protected bool _isConnectionActive = false;
	protected bool _isInitialized = false;

	// EOS Interfaces
	internal PlatformInterface _platformInterface;
	private AuthInterface _authInterface;
	private P2PInterface _p2pInterface;
	private ConnectInterface _connectInterface;

	private Dictionary<ProductUserId, EOSConnection> _connectionDictionary = new Dictionary<ProductUserId, EOSConnection>();

	private const float c_PlatformTickInterval = 0.1f;
	private float m_PlatformTickTimer = 0f;

	public static ProductUserId LocalUserId;
	public static EpicAccountId LocalAccountId;

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
			InitializeEOS();

			_voiceManager = new UnityVoiceManager();
			_voiceManager.Enable();

			_matchmaker = new EOSMatchmaker(_platformInterface.GetLobbyInterface());

			_p2pInterface = _platformInterface.GetP2PInterface();

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

	private void InitializeEOS()
	{
		var initializeOptions = new Epic.OnlineServices.Platform.InitializeOptions()
		{
			ProductName = ProductName,
			ProductVersion = ProductVersion
		};

		var initializeResult = Epic.OnlineServices.Platform.PlatformInterface.Initialize(ref initializeOptions);
		if (initializeResult != Result.Success)
		{
			throw new Exception("Failed to initialize platform: " + initializeResult);
		}

		Epic.OnlineServices.Logging.LoggingInterface.SetLogLevel(Epic.OnlineServices.Logging.LogCategory.AllCategories, logLevel);
        Epic.OnlineServices.Logging.LoggingInterface.SetCallback((ref Epic.OnlineServices.Logging.LogMessage logMessage) =>
        {
            FusionLogger.Log(logMessage.Message);
        });


        var options = new Epic.OnlineServices.Platform.Options()
		{
			ProductId = ProductId,
			SandboxId = SandboxId,
			DeploymentId = DeploymentId,
			ClientCredentials = new Epic.OnlineServices.Platform.ClientCredentials()
			{
				ClientId = ClientId,
				ClientSecret = ClientSecret
			}
		};

		_platformInterface = Epic.OnlineServices.Platform.PlatformInterface.Create(ref options);
		if (_platformInterface == null)
		{
			throw new Exception("Failed to create platform");
		}

		FusionLogger.Log("EOS Platform initialized successfully.");

		_authInterface = _platformInterface.GetAuthInterface();
		if (_authInterface == null)
		{
			throw new Exception("Failed to get auth interface");
		}

		var authLoginOptions = new Epic.OnlineServices.Auth.LoginOptions()
		{
			Credentials = new Epic.OnlineServices.Auth.Credentials()
			{
				Type = Epic.OnlineServices.Auth.LoginCredentialType.PersistentAuth,
				Id = null,
				Token = null
			},
			ScopeFlags = Epic.OnlineServices.Auth.AuthScopeFlags.BasicProfile |
						 Epic.OnlineServices.Auth.AuthScopeFlags.Presence |
						 Epic.OnlineServices.Auth.AuthScopeFlags.FriendsList |
                         Epic.OnlineServices.Auth.AuthScopeFlags.Country
        };

		_authInterface.Login(ref authLoginOptions, null, OnAuthLoginComplete);
	}

	private void OnAuthLoginComplete(ref Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo)
	{
		if (loginCallbackInfo.ResultCode == Result.Success)
		{
			FusionLogger.Log("Auth Login succeeded using persistent token.");
			LocalAccountId = loginCallbackInfo.LocalUserId;
			SetupConnectLogin();
		}
		else
		{
			FusionLogger.Log("Persistent auth failed, falling back to account portal login.");

			var portalLoginOptions = new Epic.OnlineServices.Auth.LoginOptions()
			{
				Credentials = new Epic.OnlineServices.Auth.Credentials()
				{
					Type = Epic.OnlineServices.Auth.LoginCredentialType.AccountPortal,
					Id = null,
					Token = null
				},
				ScopeFlags = Epic.OnlineServices.Auth.AuthScopeFlags.BasicProfile |
							 Epic.OnlineServices.Auth.AuthScopeFlags.Presence |
							 Epic.OnlineServices.Auth.AuthScopeFlags.FriendsList |
							 Epic.OnlineServices.Auth.AuthScopeFlags.Country
			};

			_authInterface.Login(ref portalLoginOptions, null, OnPortalLoginComplete);
		}
	}

	private void OnPortalLoginComplete(ref Epic.OnlineServices.Auth.LoginCallbackInfo portalLoginCallbackInfo)
	{
		if (portalLoginCallbackInfo.ResultCode == Result.Success)
		{
			FusionLogger.Log("Auth Login succeeded via account portal.");
			LocalAccountId = portalLoginCallbackInfo.LocalUserId;
			SetupConnectLogin();
		}
		else
		{
			FusionLogger.Error("All login attempts failed: " + portalLoginCallbackInfo.ResultCode);
		}
	}

	private void SetupConnectLogin()
	{
		_connectInterface = _platformInterface.GetConnectInterface();

		if (LocalAccountId != null && _connectInterface != null)
		{
			var copyIdTokenOptions = new Epic.OnlineServices.Auth.CopyIdTokenOptions { AccountId = LocalAccountId };
			var idTokenResult = _authInterface.CopyIdToken(ref copyIdTokenOptions, out var idToken);

			if (idTokenResult == Result.Success && idToken != null)
			{
				FusionLogger.Log($"Using Auth ID Token for Connect Login");

				var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions
				{
					Credentials = new Epic.OnlineServices.Connect.Credentials
					{
						Type = Epic.OnlineServices.ExternalCredentialType.EpicIdToken,
						Token = idToken.Value.JsonWebToken
					},
				};

				_connectInterface.Login(ref connectLoginOptions, null, OnConnectLoginComplete);
			}
			else
			{
				FusionLogger.Error($"Failed to get Auth ID token for Connect login: {idTokenResult}");
			}
		}
		else
		{
			FusionLogger.Error("LocalAccountId or ConnectInterface is null after Auth login.");
		}
	}

	private void OnConnectLoginComplete(ref Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo)
	{
		if (connectLoginCallbackInfo.ResultCode == Result.Success)
		{
			LocalUserId = connectLoginCallbackInfo.LocalUserId;
			FusionLogger.Log($"Connect login successful. ProductUserId: {LocalUserId}");
			PlayerIDManager.SetLongID((ulong)LocalUserId.ToString().GetHashCode());
			LocalPlayer.Username = $"{LocalUserId?.ToString()}"; // Fix this at some point

			ConfigureP2P();
		}
		else if (connectLoginCallbackInfo.ResultCode == Result.InvalidUser)
		{
			if (connectLoginCallbackInfo.ContinuanceToken != null)
			{
				FusionLogger.Log("New user needs to be created with continuance token");

				var createUserOptions = new Epic.OnlineServices.Connect.CreateUserOptions
				{
					ContinuanceToken = connectLoginCallbackInfo.ContinuanceToken
				};

				_connectInterface.CreateUser(ref createUserOptions, null, OnCreateUserComplete);
			}
		}
		else
		{
			FusionLogger.Error($"Connect login failed: {connectLoginCallbackInfo.ResultCode}");
		}
	}

	private void OnCreateUserComplete(ref Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo)
	{
		if (createUserCallbackInfo.ResultCode == Result.Success)
		{
			LocalUserId = createUserCallbackInfo.LocalUserId;
			FusionLogger.Log($"New user created successfully. ProductUserId: {LocalUserId}");
			PlayerIDManager.SetLongID((ulong)LocalUserId.ToString().GetHashCode());
			LocalPlayer.Username = $"{LocalUserId?.ToString()}"; // Fix this at some point

			ConfigureP2P();
		}
		else
		{
			FusionLogger.Error($"Failed to create new user: {createUserCallbackInfo.ResultCode}");
		}
	}

    private void ConfigureP2P()
	{
		if (_p2pInterface == null || LocalUserId == null)
		{
			return;
		}

		var natOptions = new Epic.OnlineServices.P2P.SetRelayControlOptions
		{
			RelayControl = Epic.OnlineServices.P2P.RelayControl.AllowRelays
		};

		_p2pInterface.SetRelayControl(ref natOptions);

		var portRangeOptions = new Epic.OnlineServices.P2P.SetPortRangeOptions
		{
			Port = 7777,
			MaxAdditionalPortsToTry = 99
		};

		_p2pInterface.SetPortRange(ref portRangeOptions);
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

		CloseAllConnections();

		if (_platformInterface != null)
		{
			_platformInterface.Release();
			_platformInterface = null;
		}

		_isInitialized = false;
	}

	private void CloseAllConnections()
	{
		if (_p2pInterface != null && LocalUserId != null)
		{
			foreach (var connection in _connectionDictionary.Values)
			{
				connection.Close();
			}

			_connectionDictionary.Clear();

			var closeOptions = new Epic.OnlineServices.P2P.CloseConnectionsOptions
			{
				LocalUserId = LocalUserId,
				SocketId = EOSSocketHandler.SocketId
			};

			_p2pInterface.CloseConnections(ref closeOptions);
		}
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
		if (_platformInterface != null)
		{
			m_PlatformTickTimer += TimeUtilities.DeltaTime;

			if (m_PlatformTickTimer >= c_PlatformTickInterval)
			{
				m_PlatformTickTimer = 0;
				_platformInterface.Tick();
			}
		}

		try
		{
			EOSSocketHandler.ReceiveMessages(_p2pInterface, ReceiveBufferSize);
		}
		catch (Exception e)
		{
			FusionLogger.LogException("receiving data on EOS P2P", e);
		}
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

	public override string GetUsername(ulong userId)
	{
		if (_platformInterface == null || LocalAccountId == null)
		{
			return "Unknown";
		}

		var userInfoInterface = _platformInterface.GetUserInfoInterface();
		if (userInfoInterface != null)
		{
			var userInfoOptions = new Epic.OnlineServices.UserInfo.QueryUserInfoOptions
			{
				LocalUserId = LocalAccountId,
				TargetUserId = LocalAccountId
			};

			userInfoInterface.QueryUserInfo(ref userInfoOptions, null, (ref Epic.OnlineServices.UserInfo.QueryUserInfoCallbackInfo callbackInfo) =>
			{
				if (callbackInfo.ResultCode == Result.Success)
				{
					var copyOptions = new Epic.OnlineServices.UserInfo.CopyUserInfoOptions
					{
						LocalUserId = LocalAccountId,
						TargetUserId = LocalAccountId
					};

					if (userInfoInterface.CopyUserInfo(ref copyOptions, out var userInfo) == Result.Success)
					{
						LocalPlayer.Username = userInfo?.DisplayName ?? "EOS User";
					}
				}
			});
		}

		return LocalPlayer.Username ?? "EOS User";
	}

	public override bool IsFriend(ulong userId)
	{
		return userId == PlayerIDManager.LocalPlatformID;
	}

	public override void BroadcastMessage(NetworkChannel channel, NetMessage message)
	{
		if (IsHost)
		{
			EOSSocketHandler.BroadcastToClients(_p2pInterface, channel, message, _connectionDictionary.Keys.ToList());
		}
		else
		{
			var hostConnection = _connectionDictionary.Values.FirstOrDefault(c => c.IsHost);
			if (hostConnection != null)
			{
				EOSSocketHandler.SendMessageToConnection(_p2pInterface, hostConnection.RemoteUserId, channel, message);
			}
		}
	}

	public override void SendToServer(NetworkChannel channel, NetMessage message)
	{
		var hostConnection = _connectionDictionary.Values.FirstOrDefault(c => c.IsHost);
		if (hostConnection != null)
		{
			EOSSocketHandler.SendMessageToConnection(_p2pInterface, hostConnection.RemoteUserId, channel, message);
		}
	}

	public override void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
	{
		var playerID = PlayerIDManager.GetPlayerID(userId);
		if (playerID != null && playerID.PlatformID != 0)
		{
			SendFromServer(playerID.PlatformID, channel, message);
		}
	}

	public override void SendFromServer(ulong userId, NetworkChannel channel, NetMessage message)
	{
		if (!IsHost)
		{
			return;
		}

		foreach (var pair in _connectionDictionary)
		{
			if (pair.Value.PlatformId == userId)
			{
				EOSSocketHandler.SendMessageToConnection(_p2pInterface, pair.Key, channel, message);
				break;
			}
		}
	}

	public override void StartServer()
	{
		if (!_isInitialized || LocalUserId == null)
		{
			FusionLogger.Error("Cannot start server: EOS not initialized or LocalUserId is null");
			return;
		}

		ConfigureP2PSocketToAcceptConnections();

		CreateLobby();

		_isServerActive = true;
		_isConnectionActive = true;

		AddConnection(LocalUserId, true);

		InternalServerHelpers.OnStartServer();

		RefreshServerCode();
	}

	private void ConfigureP2PSocketToAcceptConnections()
	{
		if (_p2pInterface == null || LocalUserId == null)
		{
			return;
		}

		var portRangeOptions = new Epic.OnlineServices.P2P.SetPortRangeOptions
		{
			Port = 7777,
			MaxAdditionalPortsToTry = 99
		};

		_p2pInterface.SetPortRange(ref portRangeOptions);

		var relayOptions = new Epic.OnlineServices.P2P.SetRelayControlOptions
		{
			RelayControl = Epic.OnlineServices.P2P.RelayControl.AllowRelays
		};

		_p2pInterface.SetRelayControl(ref relayOptions);
	}

	public override void Disconnect(string reason = "")
	{
		if (!_isServerActive && !_isConnectionActive)
			return;

		CloseAllConnections();

		if (_currentLobby != null && _platformInterface != null)
		{
			var lobbyInterface = _platformInterface.GetLobbyInterface();
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
					});
				}
			}

			_currentLobby = null;
		}

		_isServerActive = false;
		_isConnectionActive = false;

		InternalServerHelpers.OnDisconnect(reason);
	}

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

		if (!_isInitialized || LocalUserId == null || _p2pInterface == null)
		{
			FusionLogger.Error("Cannot join server: EOS not initialized or interfaces are null");
			return;
		}

		AddConnection(hostId, true);

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

		if (IsHost && LocalUserId != null)
		{
			_currentLobby.SetMetadata("host_id", LocalUserId.ToString());
		}
	}

	private void CreateLobby()
	{
		if (_platformInterface == null || LocalUserId == null)
		{
			FusionLogger.Error("Cannot create lobby: Platform interface or LocalUserId is null");
			return;
		}

		var lobbyInterface = _platformInterface.GetLobbyInterface();
		if (lobbyInterface == null)
		{
			FusionLogger.Error("Cannot create lobby: LobbyInterface is null");
			return;
		}

		FusionLogger.Log("Creating EOS lobby for server...");

		var createOptions = new Epic.OnlineServices.Lobby.CreateLobbyOptions
		{
			LocalUserId = LocalUserId,
			MaxLobbyMembers = 64,
			PermissionLevel = Epic.OnlineServices.Lobby.LobbyPermissionLevel.Publicadvertised,
			BucketId = "BONELABFUSION",
			EnableRTCRoom = false,
			DisableHostMigration = true,
			AllowInvites = true,
			PresenceEnabled = true,
        };

		lobbyInterface.CreateLobby(ref createOptions, null, (ref Epic.OnlineServices.Lobby.CreateLobbyCallbackInfo info) =>
		{
			if (info.ResultCode == Result.Success)
			{
				FusionLogger.Log($"EOS Lobby created successfully with ID: {info.LobbyId}");

				var copyOptions = new Epic.OnlineServices.Lobby.CopyLobbyDetailsHandleOptions
				{
					LobbyId = info.LobbyId,
					LocalUserId = LocalUserId
				};

				Result result = lobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);
				if (result == Result.Success && lobbyDetails != null)
				{
					_currentLobby = new EOSLobby(lobbyDetails, LocalUserId, info.LobbyId);

					_currentLobby.SetMetadata(LobbyConstants.HasServerOpenKey, bool.TrueString);
					_currentLobby.SetMetadata("host_id", LocalUserId.ToString());
					_currentLobby.SetMetadata("server_name", $"Fusion Server by {LocalPlayer.Username}");

					if (ServerCode != null)
					{
						_currentLobby.SetMetadata("lobby_code", ServerCode);
					}

					_currentLobby.WriteKeyCollection();

					LobbyInfoManager.PushLobbyUpdate();
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

	public EOSConnection AddConnection(ProductUserId userId, bool isHost = false)
	{
		if (userId == null || _p2pInterface == null || LocalUserId == null)
		{
			return null;
		}

		if (_connectionDictionary.ContainsKey(userId))
		{
			return _connectionDictionary[userId];
		}

		var connection = new EOSConnection(userId, LocalUserId.ToString().GetHashCode() == userId.ToString().GetHashCode(), isHost);
		_connectionDictionary[userId] = connection;

		EOSSocketHandler.InitializeP2PConnection(_p2pInterface, LocalUserId, userId);

		FusionLogger.Log($"Added connection to {userId} (Host: {isHost})");

		return connection;
	}

	public void RemoveConnection(ProductUserId userId)
	{
		if (userId == null || !_connectionDictionary.ContainsKey(userId))
		{
			return;
		}

		var connection = _connectionDictionary[userId];
		connection.Close();

		_connectionDictionary.Remove(userId);

		if (_p2pInterface != null && LocalUserId != null)
		{
			EOSSocketHandler.CloseP2PConnection(_p2pInterface, LocalUserId, userId);
		}

		FusionLogger.Log($"Removed connection to {userId}");

		if (IsHost && !connection.IsLocal)
		{
			InternalServerHelpers.OnPlayerLeft(connection.PlatformId);
			ConnectionSender.SendDisconnect(connection.PlatformId);
		}
	}

	public void HandleP2PConnectionRequest(ProductUserId remoteUserId)
	{
		if (remoteUserId == null || LocalUserId == null || _p2pInterface == null)
		{
			return;
		}

		FusionLogger.Log($"Handling P2P connection request from {remoteUserId}");

		if (!IsHost && !_connectionDictionary.Values.Any(c => c.IsHost && c.RemoteUserId.ToString() == remoteUserId.ToString()))
		{
			FusionLogger.Log($"Rejecting P2P connection from {remoteUserId} - not host or authorized connection");
			return;
		}

		var acceptOptions = new Epic.OnlineServices.P2P.AcceptConnectionOptions
		{
			LocalUserId = LocalUserId,
			RemoteUserId = remoteUserId,
			SocketId = EOSSocketHandler.SocketId
		};

		Result result = _p2pInterface.AcceptConnection(ref acceptOptions);
		if (result == Result.Success)
		{
			FusionLogger.Log($"Accepted P2P connection from {remoteUserId}");

			if (!_connectionDictionary.ContainsKey(remoteUserId))
			{
				AddConnection(remoteUserId, false);
			}
		}
		else
		{
			FusionLogger.Error($"Failed to accept P2P connection from {remoteUserId}: {result}");
		}
	}
}