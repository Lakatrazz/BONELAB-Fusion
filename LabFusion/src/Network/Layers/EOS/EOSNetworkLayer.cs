using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.UserInfo;
using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using LabFusion.Voice;
using LabFusion.Voice.Unity;
using Steamworks;
using Steamworks.Data;

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

    //public SteamId SteamId;

    //public static SteamSocketManager SteamSocket;
    //public static SteamConnectionManager SteamConnection;

    protected bool _isServerActive = false;
    protected bool _isConnectionActive = false;

    protected ulong _targetServerId;

    protected string _targetJoinId;

    protected bool _isInitialized = false;

    // A local reference to a lobby
    // This isn't actually used for joining servers, just for matchmaking
    protected Lobby _localLobby;

    private PlatformInterface platformInterface;
    private AuthInterface authInterface;
    private UserInfoInterface userInfoInterface;
    private const float c_PlatformTickInterval = 0.1f;
    private float m_PlatformTickTimer = 0f;

    public static Epic.OnlineServices.P2P.P2PInterface P2PInterface;
    public static Epic.OnlineServices.ProductUserId LocalUserId;
    public static Epic.OnlineServices.Sessions.SessionsInterface SessionsInterface;

    private Epic.OnlineServices.Sessions.ActiveSession _activeSession;
    private string _sessionName;

    public override bool CheckSupported()
    {
        return true;

        return !PlatformHelper.IsAndroid;
    }

    public override bool CheckValidation()
    {
        return EOSSDKLoader.HasEOSSDK;
    }

    public override void OnInitializeLayer()
    {
        // Set these values as appropriate. For more information, see the Developer Portal documentation.
        string productName = "BONELAB Fusion TEST";
        string productVersion = "1.0";
        string productId = "29e074d5b4724f3bb01f26b7e33d2582";
        string sandboxId = "26f32d66d87f4dfeb4a7449b776a41f1";
        string deploymentId = "1dffb21201e04ad89b0e6e415f0b8993";
        string clientId = "xyza7891gWLwVJx3rdLOLs6vJ05u9jWT";
        string clientSecret = "IWrUy1Z62wWajAX37k3zkQ4Kkto+AvfQSyZ9zfvibzw";

        var initializeOptions = new Epic.OnlineServices.Platform.InitializeOptions()
        {
            ProductName = productName,
            ProductVersion = productVersion
        };

        var initializeResult = Epic.OnlineServices.Platform.PlatformInterface.Initialize(ref initializeOptions);
        if (initializeResult != Epic.OnlineServices.Result.Success)
        {
            throw new System.Exception("Failed to initialize platform: " + initializeResult);
        }

        // The SDK outputs lots of information that is useful for debugging.
        // Make sure to set up the logging interface as early as possible: after initializing.
        Epic.OnlineServices.Logging.LoggingInterface.SetLogLevel(Epic.OnlineServices.Logging.LogCategory.AllCategories, Epic.OnlineServices.Logging.LogLevel.VeryVerbose);
        Epic.OnlineServices.Logging.LoggingInterface.SetCallback((ref Epic.OnlineServices.Logging.LogMessage logMessage) =>
        {
            FusionLogger.Log(logMessage.Message);
        });

        var options = new Epic.OnlineServices.Platform.Options()
        {
            ProductId = productId,
            SandboxId = sandboxId,
            DeploymentId = deploymentId,
            ClientCredentials = new Epic.OnlineServices.Platform.ClientCredentials()
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            }
        };

        platformInterface = Epic.OnlineServices.Platform.PlatformInterface.Create(ref options);
        if (platformInterface == null)
        {
            throw new System.Exception("Failed to create platform");
        }

        FusionLogger.Log("EOS Platform initialized successfully.");

        var loginCredentialType = Epic.OnlineServices.Auth.LoginCredentialType.AccountPortal;
        /// These fields correspond to <see cref="Epic.OnlineServices.Auth.Credentials.Id" /> and <see cref="Epic.OnlineServices.Auth.Credentials.Token" />,
        /// and their use differs based on the login type. For more information, see <see cref="Epic.OnlineServices.Auth.Credentials" />
        /// and the Auth Interface documentation.
        string loginCredentialId = null;
        string loginCredentialToken = null;

        authInterface = platformInterface.GetAuthInterface();
        if (authInterface == null)
        {
            throw new System.Exception("Failed to get auth interface");
        }

        var loginOptions = new Epic.OnlineServices.Auth.LoginOptions()
        {
            Credentials = new Epic.OnlineServices.Auth.Credentials()
            {
                Type = loginCredentialType,
                Id = loginCredentialId,
                Token = loginCredentialToken
            },
            // Change these scopes to match the ones set up on your product on the Developer Portal.
            ScopeFlags = Epic.OnlineServices.Auth.AuthScopeFlags.BasicProfile | Epic.OnlineServices.Auth.AuthScopeFlags.Presence | Epic.OnlineServices.Auth.AuthScopeFlags.FriendsList | AuthScopeFlags.Country
        };

        // Ensure platform tick is called on an interval, or the following call will never callback.
        authInterface.Login(ref loginOptions, null, (ref Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo) =>
        {
            if (loginCallbackInfo.ResultCode == Epic.OnlineServices.Result.Success)
            {
                FusionLogger.Log("Login succeeded");
                userInfoInterface = platformInterface.GetUserInfoInterface();
            }
            else if (Epic.OnlineServices.Common.IsOperationComplete(loginCallbackInfo.ResultCode))
            {
                FusionLogger.Log("Login failed: " + loginCallbackInfo.ResultCode);
            }
        });
    }

    public override void OnDeinitializeLayer()
    {
    }

    public override void LogIn()
    {
        InvokeLoggedInEvent();
    }

    public override void LogOut()
    {
    }

    public override void OnUpdateLayer()
    {
        if (platformInterface != null)
        {
            m_PlatformTickTimer += TimeUtilities.DeltaTime;

            if (m_PlatformTickTimer >= c_PlatformTickInterval)
            {
                m_PlatformTickTimer = 0;
                platformInterface.Tick();
            }
        }
    }

    public override string GetUsername(ulong userId)
    {
        EpicAccountId epicAccountId = authInterface.GetLoggedInAccountByIndex(0);

        if (userInfoInterface != null)
        {
            var userInfoOptions = new Epic.OnlineServices.UserInfo.CopyUserInfoOptions()
            {
                LocalUserId = epicAccountId
            };
            var userInfoResult = userInfoInterface.CopyUserInfo(ref userInfoOptions, out UserInfoData? userInfo);
            if (userInfoResult == Epic.OnlineServices.Result.Success)
            {
                return userInfo?.DisplayName;
            }
        }
        return null;
    }

    public override bool IsFriend(ulong userId)
    {
        return false;

        return userId == PlayerIDManager.LocalPlatformID || new Friend(userId).IsFriend;
    }

    public override void BroadcastMessage(NetworkChannel channel, NetMessage message)
    {
    }

    public override void SendToServer(NetworkChannel channel, NetMessage message)
    {
    }

    public override void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
    {
    }

    public override void SendFromServer(ulong userId, NetworkChannel channel, NetMessage message)
    {
    }

    public override void StartServer()
    {

    }

    public override void Disconnect(string reason = "")
    {
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
    }
}