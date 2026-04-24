using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using LabFusion.Utilities;
using System.Collections;
using MelonLoader;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Manages EOS authentication flow.
/// </summary>
internal class EOSAuthManager
{
    internal const string UnknownDisplayName = "Unknown";
    
    internal ProductUserId LocalUserId { get; private set; }
    internal EOSAuthInterface authInterface { get; set; }
    private bool IsLoggedIn => LocalUserId != null;
    
    private ulong _expirationNotificationId;

    internal EOSAuthManager()
    {
        var platform = PlatformHelper.GetPlatform();

        switch (platform)
        {
            case PlatformHelper.Platform.Steam:
                authInterface = new EOSSteamAuth();
                break;
            case PlatformHelper.Platform.Rift:
            case PlatformHelper.Platform.Quest:
                authInterface = new EOSOculusAuth();
                break;
            default:
                throw new NotSupportedException($"Platform {platform} not supported");
        }
    }

    internal IEnumerator LoginAsync(Action<bool> onComplete)
    {
        bool loginSuccess = false;

        yield return LoginWithInterfaceAsync(success => loginSuccess = success);

        if (loginSuccess)
        {
            RegisterAuthExpiration();
        }

        onComplete?.Invoke(loginSuccess);
    }

    internal void Shutdown()
    {
        authInterface?.OnShutdown();
    }

    internal IEnumerator GetDisplayNameAsync(Action<string> onComplete)
    {
        if (!IsLoggedIn)
        {
            onComplete?.Invoke(UnknownDisplayName);
            yield break;
        }
        
        string displayName = null;
        yield return authInterface.GetDisplayNameAsync(name => displayName = name);

        onComplete?.Invoke(!string.IsNullOrEmpty(displayName) ? displayName : UnknownDisplayName);
    }

    private IEnumerator LoginWithInterfaceAsync(Action<bool> onComplete)
    {
        var connect = EOSInterfaces.Connect;
        if (connect == null || authInterface == null)
        {
            FusionLogger.Error("ConnectInterface or AuthInterface is null");
            onComplete?.Invoke(false);
            yield break;
        }

        string platformToken = null;
        yield return authInterface.GetLoginTicketAsync(token => platformToken = token);

        if (!authInterface.AllowNullToken && string.IsNullOrEmpty(platformToken))
        {
            FusionLogger.Error($"Failed to retrieve token for {authInterface.AccountType}");
            onComplete?.Invoke(false);
            yield break;
        }

        bool finished = false;
        bool success = false;
        ContinuanceToken continuanceToken = null;

        var loginOptions = new LoginOptions
        {
            Credentials = new Credentials
            {
                Type = authInterface.CredentialType,
                Token = platformToken,
            }
        };

        if (authInterface.LoginWithDisplayName)
        {
            string displayName = null;
            yield return GetDisplayNameAsync(name => displayName = name);
            
            loginOptions.UserLoginInfo = new UserLoginInfo
            {
                DisplayName = displayName
            };
        }

        connect.Login(ref loginOptions, null, (ref LoginCallbackInfo data) =>
        {
            switch (data.ResultCode)
            {
                case Result.Success:
                    LocalUserId = data.LocalUserId;
                    success = true;
                    finished = true;
                    break;
                case Result.InvalidUser:
                    continuanceToken = data.ContinuanceToken;
                    break;
                default:
                    FusionLogger.Error($"EOS Login failed: {data.ResultCode}");
                    finished = true;
                    break;
            }
        });

        while (!finished && continuanceToken == null)
            yield return null;

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
        bool finished = false;
        bool success = false;
        var options = new CreateUserOptions { ContinuanceToken = token };

        connect.CreateUser(ref options, null, (ref CreateUserCallbackInfo data) =>
        {
            if (data.ResultCode == Result.Success)
            {
                LocalUserId = data.LocalUserId;
                success = true;
            }
            else
            {
                FusionLogger.Error($"EOS CreateUser failed: {data.ResultCode}");
            }
            finished = true;
        });

        while (!finished) yield return null;
        onComplete?.Invoke(success);
    }

    private void RegisterAuthExpiration()
    {
        UnregisterAuthExpiration();

        var expirationOptions = new AddNotifyAuthExpirationOptions();
        _expirationNotificationId = EOSInterfaces.Connect.AddNotifyAuthExpiration(ref expirationOptions, null, (ref AuthExpirationCallbackInfo _) =>
            {
#if DEBUG
                FusionLogger.Log("EOS token expiring - starting refresh...");
#endif
                MelonCoroutines.Start(RefreshTokenAsync());
            }
        );
    }

    private void UnregisterAuthExpiration()
    {
        if (_expirationNotificationId != 0)
        {
            EOSInterfaces.Connect.RemoveNotifyAuthExpiration(_expirationNotificationId);
            _expirationNotificationId = 0;
        }
    }

    private IEnumerator RefreshTokenAsync()
    {
#if DEBUG
        FusionLogger.Log("Refreshing EOS token...");
#endif

        bool success = false;
        yield return LoginWithInterfaceAsync(result => success = result);
 
        if (success)
        {
#if DEBUG
            FusionLogger.Log("EOS token refreshed successfully.");
#endif
            
            RegisterAuthExpiration();
        }
        else
        {
            FusionLogger.Error("EOS token refresh failed - user may need to re-authenticate.");
            LocalUserId = null;
        }
    }
}