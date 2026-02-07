using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

using LabFusion.Utilities;

using System.Collections;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Manages EOS authentication flow.
/// </summary>
internal class EOSAuthManager
{
    private readonly EOSDeviceIdAuth _deviceIdAuth;

    public ProductUserId LocalUserId { get; private set; }
    public bool IsLoggedIn => LocalUserId != null;

    public EOSAuthManager()
    {
        _deviceIdAuth = new EOSDeviceIdAuth();
    }

    public IEnumerator LoginAsync(Action<bool> onComplete)
    {
        // Step 1: Create device ID
        bool deviceIdSuccess = false;

        yield return _deviceIdAuth.CreateDeviceIdAsync(success => deviceIdSuccess = success);

        if (!deviceIdSuccess)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        // Step 2: Login with device ID
        bool loginSuccess = false;

        yield return LoginWithDeviceIdAsync(success => loginSuccess = success);
        
        if (loginSuccess)
            RegisterAuthExpiration();

        onComplete?.Invoke(loginSuccess);
    }

    private IEnumerator LoginWithDeviceIdAsync(Action<bool> onComplete)
    {
        var connect = EOSInterfaces.Connect;
        if (connect == null)
        {
            FusionLogger.Error("ConnectInterface is null when logging in");
            onComplete?.Invoke(false);
            yield break;
        }

        // Get username
        string username = "Unknown";
        bool usernameRetrieved = false;

        yield return EOSUsernameDeterminer.GetUsernameAsync(s =>
        {
            username = s;
            usernameRetrieved = true;
        });

        while (!usernameRetrieved)
            yield return null;

        // Attempt login
        bool finished = false;
        bool success = false;
        ContinuanceToken continuanceToken = null;

        var loginOptions = new LoginOptions
        {
            Credentials = new Credentials
            {
                Type = ExternalCredentialType.DeviceidAccessToken,
                Token = null,
            },
            UserLoginInfo = new UserLoginInfo
            {
                DisplayName = username
            },
        };

        connect.Login(ref loginOptions, null, (ref LoginCallbackInfo data) =>
        {
            switch (data.ResultCode)
            {
                case Result.Success:
                    LocalUserId = data.LocalUserId;
#if DEBUG
                    FusionLogger.Log($"Logged in successfully! PUID = {LocalUserId}");
#endif
                    success = true;
                    finished = true;
                    break;

                case Result.InvalidUser:
                    continuanceToken = data.ContinuanceToken;
                    break;

                default:
                    FusionLogger.Error($"Login failed: {data.ResultCode}");
                    finished = true;
                    break;
            }
        });

        while (!finished && continuanceToken == null)
            yield return null;

        // Create user if needed
        if (continuanceToken != null)
        {
            yield return CreateUserAsync(continuanceToken, result =>
            {
                success = result;
                finished = true;
            });
        }

        while (!finished)
            yield return null;

        onComplete?.Invoke(success);
    }

    private IEnumerator CreateUserAsync(ContinuanceToken token, Action<bool> onComplete)
    {
        var connect = EOSInterfaces.Connect;
        if (connect == null)
        {
            FusionLogger.Error("ConnectInterface is null when creating user");
            onComplete?.Invoke(false);
            yield break;
        }

        bool finished = false;
        bool success = false;

        var options = new CreateUserOptions
        {
            ContinuanceToken = token
        };

        connect.CreateUser(ref options, null, (ref CreateUserCallbackInfo data) =>
        {
            if (data.ResultCode == Result.Success)
            {
                LocalUserId = data.LocalUserId;
                FusionLogger.Log($"User created successfully! PUID = {LocalUserId}");
                success = true;
            }
            else
            {
                FusionLogger.Error($"CreateUser failed: {data.ResultCode}");
            }
            finished = true;
        });

        while (!finished)
            yield return null;

        onComplete?.Invoke(success);
    }

    private static void RegisterAuthExpiration()
    {
        var notifyAuthExpirationOptions = new AddNotifyAuthExpirationOptions();
        EOSInterfaces.Connect.AddNotifyAuthExpiration(ref notifyAuthExpirationOptions, null, AuthExpirationCallback);
    } 
    
    private static void AuthExpirationCallback(ref AuthExpirationCallbackInfo data)
    {
        var loginOptions = new LoginOptions
        {
            Credentials = new Credentials
            {
                Type = ExternalCredentialType.DeviceidAccessToken,
                Token = null,
            },
            UserLoginInfo = new UserLoginInfo
            {
                DisplayName = EOSUsernameDeterminer.CachedUsername
            },
        };
        
        EOSInterfaces.Connect.Login(ref loginOptions, null, (ref LoginCallbackInfo data) =>
        {
            switch (data.ResultCode)
            {
                case Result.Success:
#if DEBUG
                    FusionLogger.Log("Token refreshed!");
#endif
                    break;
                default:
                    FusionLogger.Error($"Token refresh failed with result: {data.ResultCode}");
                    break;
            }
        });
    }
}