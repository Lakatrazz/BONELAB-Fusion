using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using LabFusion.Utilities;
using UnityEngine;

namespace LabFusion.Network.EpicGames;

internal class EOSOculusAuth : EOSAuthInterface
{
    internal override ExternalAccountType AccountType => ExternalAccountType.Oculus;

    internal override ExternalCredentialType CredentialType => ExternalCredentialType.DeviceidAccessToken;

    internal override bool AllowNullToken => true;

    internal override bool LoginWithDisplayName => true;
    
    internal override IEnumerator GetDisplayNameAsync(Action<string> onDisplayNameReceived)
    {
        string displayName = null;
        bool requestComplete = false;
        Il2CppOculus.Platform.Users.GetLoggedInUser().OnComplete((Il2CppOculus.Platform.Message<Il2CppOculus.Platform.Models.User>.Callback)(message =>
        {
            if (!message.IsError)
                displayName = message.Data.OculusID;
            
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