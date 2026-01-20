using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

using UnityEngine;

namespace LabFusion.Network.EpicGames;

internal class EOSAuthManager
{
    internal EOSAuthManager()
    {

    }

    internal ProductUserId puid;

    internal IEnumerator LoginAsync(System.Action<bool> onComplete)
    {
        bool finished = false;
        bool success = false;

        yield return CreateDeviceIDAsync();

        while (!finished)
            yield return null;

        onComplete?.Invoke(success);

        IEnumerator CreateDeviceIDAsync()
        {
            ConnectInterface connect = EOSManager.ConnectInterface;

            CreateDeviceIdOptions options = new CreateDeviceIdOptions
            {
                DeviceModel = SystemInfo.deviceModel,
            };

            connect.CreateDeviceId(ref options, null, (ref CreateDeviceIdCallbackInfo data) =>
            {
                if (data.ResultCode == Result.Success || data.ResultCode == Result.DuplicateNotAllowed)
                {
                    MelonCoroutines.Start(LoginWithDeviceIdAsync());
                }
                else
                {
                    FusionLogger.Error($"CreateDeviceId failed: {data.ResultCode}");
                    finished = true;
                }
            });

            yield break;
        }

        IEnumerator LoginWithDeviceIdAsync()
        {
            ConnectInterface connect = EOSManager.ConnectInterface;

            string username = "Unknown";
            yield return EOSUsernameDeterminer.GetUsernameAsync(s => username = s);

            LoginOptions loginOptions = new LoginOptions
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
                if (data.ResultCode == Result.Success)
                {
                    puid = data.LocalUserId;
                    FusionLogger.Log($"Logged in successfully! PUID = {puid}");
                    success = true;
                    finished = true;
                }
                else if (data.ResultCode == Result.InvalidUser)
                {
                    CreateUser(data.ContinuanceToken);
                }
                else
                {
                    FusionLogger.Error($"Login failed: {data.ResultCode}");
                    finished = true;
                }
            });
        }

        void CreateUser(ContinuanceToken token)
        {
            ConnectInterface connect = EOSManager.ConnectInterface;

            CreateUserOptions options = new CreateUserOptions
            {
                ContinuanceToken = token
            };

            connect.CreateUser(ref options, null, (ref CreateUserCallbackInfo data) =>
            {
                if (data.ResultCode == Result.Success)
                {
                    puid = data.LocalUserId;
                    FusionLogger.Log($"User created successfully! PUID = {puid}");
                    success = true;
                }
                else
                {
                    FusionLogger.Error($"CreateUser failed: {data.ResultCode}");
                }

                finished = true;
            });
        }
    }
}