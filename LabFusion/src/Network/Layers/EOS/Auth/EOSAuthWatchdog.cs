using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

using LabFusion.Utilities;

namespace LabFusion.Network;

// thx epic for actually giving sample code for once
internal class EOSAuthWatchdog
{
    static ulong authExpirationNotifyId = Common.InvalidNotificationid;

    internal static void SetupWatchdog()
    {
        var notifyAuthExpirationOptions = new AddNotifyAuthExpirationOptions();

        authExpirationNotifyId = EOSManager.ConnectInterface.AddNotifyAuthExpiration(ref notifyAuthExpirationOptions, null, AuthExpirationCallback);
    }

    internal static void ShutdownWatchdog()
    {
        if (authExpirationNotifyId != Common.InvalidNotificationid)
        {
            EOSManager.ConnectInterface.RemoveNotifyAuthExpiration(authExpirationNotifyId);
            authExpirationNotifyId = Common.InvalidNotificationid;
        }
    }

    private static void AuthExpirationCallback(ref AuthExpirationCallbackInfo data)
    {
        // Handle 10-minute warning prior to token expiration by calling Connect.Login()
        TokenRefresh();
    }

    // need better method name
    private static void TokenRefresh()
    {
        var copyIdTokenOptions = new Epic.OnlineServices.Auth.CopyIdTokenOptions
        {
            AccountId = EOSNetworkLayer.LocalAccountId,
        };
        var idTokenResult = EOSManager.AuthInterface.CopyIdToken(ref copyIdTokenOptions, out var idToken);

        if (idTokenResult != Result.Success)
        {
            FusionLogger.Error($"Failed to get Auth ID token for Connect login: {idTokenResult}");
            return;
        }

        var connectLoginOptions = new LoginOptions
        {
            Credentials = new Credentials
            {
                Type = ExternalCredentialType.EpicIdToken,
                Token = idToken.Value.JsonWebToken,
            },
        };
        EOSManager.ConnectInterface.Login(ref connectLoginOptions, null, (ref LoginCallbackInfo connectLoginCallbackInfo) =>
        {
            if (connectLoginCallbackInfo.ResultCode == Result.Success)
            {
#if DEBUG
                FusionLogger.Log("Token refreshed!");
#endif
            }
            else
            {
                FusionLogger.Error($"Token refresh failed with result: {connectLoginCallbackInfo.ResultCode}");
            }
        });
    }
}
