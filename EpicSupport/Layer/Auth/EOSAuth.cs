using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using LabFusion.UI.Popups;
using LabFusion.Utilities;

namespace MarrowFusion.Epic;

internal class EOSAuth : EOSInterface
{
    private const string NotificationTag = "EpicGamesAuth";
    
    internal EOSRuntime Runtime;
    internal AuthInterface AuthInterface;
    internal EpicAccountId LocalAccountId;
    
    private EpicWebAPI webAPI = new EpicWebAPI();
    
    internal EOSAuth(EOSRuntime eosRuntime, AuthInterface authInterface)
    {
        Runtime = eosRuntime;
        AuthInterface = authInterface;
    }

    internal override Task<bool> InitializeAsync()
    {
        return LoginAsync();
    }
    
    private async Task<bool> LoginAsync()
    {
        if (await LoginAsync(LoginCredentialType.PersistentAuth))
            return true;
 
        if (PlatformHelper.IsAndroid)
        {
            string saved = EpicTokenStore.Get();
            if (saved != null && await LoginAsync(LoginCredentialType.RefreshToken, saved))
            {
                SaveRefreshToTokenStore();
                return true;
            }
 
            return await LoginWithDeviceCodeAsync();
        }
 
        return await LoginAsync(LoginCredentialType.AccountPortal);
    }
    
    private Task<bool> LoginAsync(LoginCredentialType type, string token = null)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
 
        var options = new LoginOptions
        {
            Credentials = new Credentials
            {
                Type = type,
                Token = token,
            },
            ScopeFlags = AuthScopeFlags.BasicProfile
        };
 
        AuthInterface.Login(ref options, null, (ref LoginCallbackInfo data) =>
        {
            if (data.ResultCode == Result.Success)
            {
                LocalAccountId = data.LocalUserId;
                Notifier.Cancel(NotificationTag);
                tcs.TrySetResult(true);
                return;
            }
 
            tcs.TrySetResult(false);
        });
 
        return tcs.Task;
    }
    
    private async Task<bool> LoginWithDeviceCodeAsync()
    {
        try
        {
            var device = await webAPI.BeginDeviceAuthAsync();
 
            EpicModule.Logger.Log($"URL: {device.VerificationUri}");
            EpicModule.Logger.Log($"Pin Code: {device.UserCode}");
            EpicModule.Logger.Log($"Expires in: {device.ExpiresIn}");
 
            OnPinCodeReceived(device.VerificationUri, device.UserCode, device.ExpiresIn);
 
            var tokens = await webAPI.WaitForDeviceAuthAsync(device);
 
            Notifier.Cancel(NotificationTag);
 
            if (await LoginAsync(LoginCredentialType.RefreshToken, tokens.RefreshToken))
            {
                SaveRefreshToTokenStore();
                return true;
            }
            
            return false;
        }
        catch (TimeoutException)
        {
            OnPinCodeExpired();
            return false;
        }
        catch (EpicWebAPIException ex)
        {
            EpicModule.Logger.LogException("device code login", ex);
            EpicModule.Logger.Log($"Response status: {ex.Status}");
            EpicModule.Logger.Log($"Response body: {ex.ResponseBody}");
            Notifier.Cancel(NotificationTag);
            return false;
        }
        catch (Exception ex)
        {
            EpicModule.Logger.LogException("device code login", ex);
            Notifier.Cancel(NotificationTag);
            return false;
        }
    }

    private void OnPinCodeReceived(string verificationUri, string userCode, int expiresIn)
    {
        var notification = new Notification()
        {
            Message = $"Please visit {verificationUri} and enter the code {userCode} to continue logging in.",
            PopupLength = expiresIn,
            ShowPopup = true,
            SaveToMenu = false,
            Tag = NotificationTag,
        };
        
        Notifier.Send(notification);
    }
    
    private void OnPinCodeExpired()
    {
        EpicModule.Logger.Warn("Your login code has expired. Please try logging in again.");
        
        Notifier.Cancel(NotificationTag);
        
        var notification = new Notification()
        {
            Message = "Your login code has expired. Please try logging in again.",
            ShowPopup = true,
            SaveToMenu = false,
            Type = NotificationType.WARNING,
        };
        
        Notifier.Send(notification);
    }
    
    private void SaveRefreshToTokenStore()
    {
        if (LocalAccountId == null)
            return;
 
        var options = new CopyUserAuthTokenOptions();
 
        if (AuthInterface.CopyUserAuthToken(ref options, LocalAccountId, out Token? token) == Result.Success && token.HasValue)
        {
            string refresh = token.Value.RefreshToken;
            if (!string.IsNullOrEmpty(refresh))
            {
                EpicTokenStore.Set(refresh);
            }
        }
    }
    
    private void DeleteAuth()
    {
        if (PlatformHelper.IsAndroid)
        {
            DeleteTokenStore();
        }
        else
        {
            DeletePersistentAuth();
        }
    }
    
    private void DeleteTokenStore()
    {
        if (LocalAccountId == null)
            return;
        
        EpicTokenStore.Clear();
    }
        
    private void DeletePersistentAuth()
    {
        if (LocalAccountId == null)
            return;
        
        var options = new DeletePersistentAuthOptions();

        // Cant just set a null completion callback. thanks epic
        AuthInterface.DeletePersistentAuth(ref options, null, (ref DeletePersistentAuthCallbackInfo data) => {});
    }
    
    // Used during connect login.
    internal string CopyIdToken()
    {
        if (LocalAccountId == null)
            return null;

        var options = new CopyIdTokenOptions
        {
            AccountId = LocalAccountId
        };

        if (AuthInterface.CopyIdToken(ref options, out IdToken? token) == Result.Success && token.HasValue)
            return token.Value.JsonWebToken;

        return null;
    }
}