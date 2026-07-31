using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using LabFusion.Utilities;
using MelonLoader;
using UnityEngine;

namespace LabFusion.Network;

internal class EOSConnect : EOSInterface
{
    internal ConnectInterface ConnectInterface;
    internal ProductUserId LocalUserId;
    internal string LocalDisplayName;
    internal ulong ExpirationNotificationId;

    internal EOSConnect(ConnectInterface connectInterface)
    {
        ConnectInterface = connectInterface;
    }
    
    internal override IEnumerator InitializeAsync(Action<bool> onComplete)
    {
        bool loginSuccess = false;
        yield return LoginAsync((success) => loginSuccess = success);
        if (!loginSuccess)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        onComplete?.Invoke(true);

        yield return null;
    }
    
    private IEnumerator LoginAsync(Action<bool> onComplete)
    {
        var createDeviceIdOptions = new CreateDeviceIdOptions
        {
            DeviceModel = SystemInfo.deviceModel,
        };

        bool createDeviceIdSuccess = false;
        bool createDeviceIdFinished = false;
        ConnectInterface.CreateDeviceId(ref createDeviceIdOptions, null, ((ref CreateDeviceIdCallbackInfo data) =>
        {
            if (data.ResultCode == Result.Success || data.ResultCode == Result.DuplicateNotAllowed)
            {
                createDeviceIdSuccess = true;
            }
            else
            {
                FusionLogger.Error($"CreateDeviceId failed: {data.ResultCode}");
            }   
            
            createDeviceIdFinished = true;
        }));

        while (!createDeviceIdFinished)
            yield return null;

        if (!createDeviceIdSuccess)
        {
            onComplete?.Invoke(false);
            yield break;
        }
        
        yield return GetDisplayNameAsync(name => LocalDisplayName = name);
        
        var loginOptions = new LoginOptions
        {
            Credentials = new Credentials
            {
                Type = ExternalCredentialType.DeviceidAccessToken, 
                Token = string.Empty,
            },
            UserLoginInfo = new UserLoginInfo
            {
                DisplayName = string.IsNullOrWhiteSpace(LocalDisplayName) ? "Unknown" : LocalDisplayName
            }
        };
        
        ContinuanceToken continuanceToken = null;
        bool loginFinished = false;
        ConnectInterface.Login(ref loginOptions, null, (ref LoginCallbackInfo data) =>
        {
            if (data.ResultCode == Result.Success)
            {
                LocalUserId = data.LocalUserId;
            }
            else if (data.ResultCode == Result.InvalidUser)
            {
                continuanceToken = data.ContinuanceToken;
            }
            else
            {
                FusionLogger.Error($"Login failed: {data.ResultCode}");
                onComplete?.Invoke(false);
            }
            
            loginFinished = true;
        });

        while (!loginFinished)
            yield return null;
        
        if (continuanceToken != null)
        {
            var createUserOptions = new CreateUserOptions
            {
                ContinuanceToken = continuanceToken
            };
            
            bool createUserFinished = false;
            ConnectInterface.CreateUser(ref createUserOptions, null, (ref CreateUserCallbackInfo data) =>
            {
                if (data.ResultCode == Result.Success)
                {
                    LocalUserId = data.LocalUserId;
                }
                else
                {
                    FusionLogger.Error($"CreateUser failed: {data.ResultCode}");
                }
                
                createUserFinished = true;
            });

            while (!createUserFinished)
                yield return null;
        }
        
        bool success = LocalUserId != null;
        if (success)
        {
            RegisterAuthExpiration();
        }
        onComplete?.Invoke(success);
        
        yield return null;
    }

    private IEnumerator GetDisplayNameAsync(Action<string> onComplete)
    {
        var platform = PlatformHelper.GetPlatform();

        switch (platform)
        {
            case PlatformHelper.Platform.Steam:
                yield return GetSteam(onComplete);
                yield break;
            case PlatformHelper.Platform.Rift:
            case PlatformHelper.Platform.Quest:
                yield return GetOculus(onComplete);
                yield break;
        }
        
        IEnumerator GetSteam(Action<string> onComplete)
        {
            const uint steamAppId = 1592190;
            
            if (!Il2CppSteamworks.SteamClient.IsValid)
                Il2CppSteamworks.SteamClient.Init(steamAppId);
            
            onComplete?.Invoke(Il2CppSteamworks.SteamClient.Name);
            
            yield return null;
        }
        
        IEnumerator GetOculus(Action<string> onComplete)
        {
            string displayName = null;
            bool requestComplete = false;
            Il2CppOculus.Platform.Users.GetLoggedInUser().OnComplete((Il2CppOculus.Platform.Message<Il2CppOculus.Platform.Models.User>.Callback)(message =>
            {
                if (!message.IsError)
                {
                    displayName = message.Data.DisplayName;
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        displayName = message.Data.OculusID;
                    }
                }
            
                requestComplete = true;
            }));

            while (!requestComplete)
                yield return null;
        
            onComplete?.Invoke(displayName);
            
            yield return null;
        }
    }
    
    private void RegisterAuthExpiration()
    {
        UnregisterAuthExpiration();
        var options = new AddNotifyAuthExpirationOptions();
        ExpirationNotificationId = ConnectInterface.AddNotifyAuthExpiration(ref options, null, (ref AuthExpirationCallbackInfo _) => MelonCoroutines.Start(RefreshTokenAsync()));
        
        IEnumerator RefreshTokenAsync()
        {
            bool success = false;
            yield return LoginAsync(result => success = result);
            if (success)
            {
                RegisterAuthExpiration();
                yield break;
            }
        
            FusionLogger.Error("Failed to refresh token, logging out...");
            NetworkLayerManager.LogOut();
        }
    }

    private void UnregisterAuthExpiration()
    {
        if (ExpirationNotificationId == Common.INVALID_NOTIFICATIONID)
            return;
        
        ConnectInterface.RemoveNotifyAuthExpiration(ExpirationNotificationId);
        ExpirationNotificationId = Common.INVALID_NOTIFICATIONID;
    }
}