using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using LabFusion.Network;
using LabFusion.Utilities;

namespace MarrowFusion.Epic;

internal class EOSConnect : EOSInterface
{
    internal EOSRuntime Runtime;
    internal ConnectInterface ConnectInterface;
    internal ProductUserId LocalUserId;
    
    private ulong ExpirationNotificationId = Common.INVALID_NOTIFICATIONID;

    internal EOSConnect(EOSRuntime eosRuntime, ConnectInterface connectInterface)
    {
        Runtime = eosRuntime;
        ConnectInterface = connectInterface;
    }

    internal override Task<bool> InitializeAsync()
    {
        return LoginAsync();
    }

    private async Task<bool> LoginAsync()
    {
        string idToken = Runtime.Auth.CopyIdToken();
        if (string.IsNullOrEmpty(idToken))
        {
            EpicModule.Logger.Error("No Epic ID token available for Connect login");
            return false;
        }
        
        var loginOptions = new LoginOptions
        {
            Credentials = new Credentials
            {
                Type = ExternalCredentialType.EpicIdToken,
                Token = idToken
            }
        };
        
        var loginResult = await LoginAsync(loginOptions);
        if (loginResult.ResultCode == Result.Success)
        {
            LocalUserId = loginResult.LocalUserId;
        }
        else if (loginResult.ResultCode == Result.InvalidUser && loginResult.ContinuanceToken != null)
        {
            var createUserOptions = new CreateUserOptions
            {
                ContinuanceToken = loginResult.ContinuanceToken
            };

            var createUserResult = await CreateUserAsync(createUserOptions);
            if (createUserResult.ResultCode != Result.Success)
            {
                EpicModule.Logger.Error($"CreateUser failed: {createUserResult.ResultCode}");
                return false;
            }

            LocalUserId = createUserResult.LocalUserId;
        }
        else
        {
            EpicModule.Logger.Error($"Login failed: {loginResult.ResultCode}");
            return false;
        }

        if (LocalUserId == null)
            return false;

        RegisterAuthExpiration();
        return true;
    }

    private Task<LoginCallbackInfo> LoginAsync(LoginOptions options)
    {
        var tcs = new TaskCompletionSource<LoginCallbackInfo>(TaskCreationOptions.RunContinuationsAsynchronously);

        ConnectInterface.Login(ref options, null, (ref LoginCallbackInfo data) =>
        {
            tcs.TrySetResult(data);
        });

        return tcs.Task;
    }

    private Task<CreateUserCallbackInfo> CreateUserAsync(CreateUserOptions options)
    {
        var tcs = new TaskCompletionSource<CreateUserCallbackInfo>(TaskCreationOptions.RunContinuationsAsynchronously);

        ConnectInterface.CreateUser(ref options, null, (ref CreateUserCallbackInfo data) =>
        {
            tcs.TrySetResult(data);
        });

        return tcs.Task;
    }

    private void RegisterAuthExpiration()
    {
        UnregisterAuthExpiration();

        var authExpirationOptions = new AddNotifyAuthExpirationOptions();

        ExpirationNotificationId = ConnectInterface.AddNotifyAuthExpiration(ref authExpirationOptions, null, (ref AuthExpirationCallbackInfo info) =>
        {
            _ = RefreshTokenAsync();
        });
    }

    private void UnregisterAuthExpiration()
    {
        if (ExpirationNotificationId == Common.INVALID_NOTIFICATIONID)
            return;

        ConnectInterface.RemoveNotifyAuthExpiration(ExpirationNotificationId);

        ExpirationNotificationId = Common.INVALID_NOTIFICATIONID;
    }

    private async Task RefreshTokenAsync()
    {
        if (await LoginAsync())
            return;

        EpicModule.Logger.Error("Failed to refresh connect token, logging out...");

        NetworkLayerManager.LogOut();
    }
    
    internal Task<EpicAccountId> GetEpicAccountIdAsync(ProductUserId productUserId)
    {
        var tcs = new TaskCompletionSource<EpicAccountId>(TaskCreationOptions.RunContinuationsAsynchronously);

        var options = new QueryProductUserIdMappingsOptions
        {
            LocalUserId = LocalUserId,
            ProductUserIds = new[] { productUserId }
        };

        ConnectInterface.QueryProductUserIdMappings(ref options, null, (ref QueryProductUserIdMappingsCallbackInfo data) =>
        {
            if (data.ResultCode != Result.Success)
            {
                tcs.TrySetResult(null);
                return;
            }

            var mappingOptions = new GetProductUserIdMappingOptions
            {
                LocalUserId = LocalUserId,
                TargetProductUserId = productUserId
            };

            var result = ConnectInterface.GetProductUserIdMapping(ref mappingOptions, out var epicAccountIdStr);
            if (result != Result.Success || string.IsNullOrEmpty(epicAccountIdStr))
            {
                tcs.TrySetResult(null);
                return;
            }
            
            var epicAccountId = EpicAccountId.FromString(epicAccountIdStr);

            tcs.TrySetResult(epicAccountId != null && epicAccountId.IsValid() ? epicAccountId : null);
        });

        return tcs.Task;
    }
}