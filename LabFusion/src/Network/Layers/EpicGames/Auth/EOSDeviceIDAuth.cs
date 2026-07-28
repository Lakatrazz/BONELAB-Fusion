using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using LabFusion.Utilities;
using UnityEngine;

namespace LabFusion.Network.EpicGames;

internal class EOSDeviceIDAuth : EOSAuthInterface
{
    internal override ExternalAccountType AccountType => ExternalAccountType.Epic;

    internal override ExternalCredentialType CredentialType => ExternalCredentialType.DeviceidAccessToken;

    internal override bool AllowNullToken => true;

    internal override bool LoginWithDisplayName => true;
    
    internal override IEnumerator GetDisplayNameAsync(Action<string> onDisplayNameReceived)
    {
        var platform = PlatformHelper.GetPlatform();

        switch (platform)
        {
            case PlatformHelper.Platform.Steam:
                yield return GetDisplayNameSteamAsync(onDisplayNameReceived);
                yield break;
            case PlatformHelper.Platform.Rift:
            case PlatformHelper.Platform.Quest:
                yield return GetDisplayNameOculusAsync(onDisplayNameReceived);
                yield break;
        }
    }
    
    private const uint steamAppId = 1592190;
    
    private IEnumerator GetDisplayNameSteamAsync(Action<string> onDisplayNameReceived)
    {
        if (!Il2CppSteamworks.SteamClient.IsValid)
            Il2CppSteamworks.SteamClient.Init(steamAppId);
        
        string displayName = Il2CppSteamworks.SteamClient.Name;
        onDisplayNameReceived?.Invoke(displayName);
        yield break;
    }
    
    private IEnumerator GetDisplayNameOculusAsync(Action<string> onDisplayNameReceived)
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
        
        onDisplayNameReceived?.Invoke(displayName);
    }

    internal override IEnumerator GetLoginTicketAsync(Action<string> onTokenReceived)
    {
        var connect = EOSInterfaces.Connect;

        bool finished = false;
        bool deviceIdReady = false;
        
        var createOptions = new CreateDeviceIdOptions
        {
            DeviceModel = SystemInfo.deviceModel,
        };

        connect.CreateDeviceId(ref createOptions, null, (ref CreateDeviceIdCallbackInfo data) =>
        {
            if (data.ResultCode == Result.Success || data.ResultCode == Result.DuplicateNotAllowed)
            {
                deviceIdReady = true;
            }
            else
            {
                FusionLogger.Error($"CreateDeviceId failed: {data.ResultCode}");
            }
            
            finished = true;
        });

        while (!finished)
            yield return null;

        if (!deviceIdReady)
        {
            onTokenReceived?.Invoke(null);
            yield break;
        }
        
        onTokenReceived?.Invoke(string.Empty);
    }
}