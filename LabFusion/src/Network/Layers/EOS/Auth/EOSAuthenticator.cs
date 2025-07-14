using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;

using LabFusion.Data;
using LabFusion.Utilities;

using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LabFusion.Network;

internal class EOSAuthenticator
{
    [System.Serializable]
    private class AuthData
    {
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }

    private const string AuthFileName = "eos_auth.dat";

    private static void SaveAuthData(EpicAccountId accountId)
    {
        var copyUserAuthTokenOptions = new CopyUserAuthTokenOptions();
        var result = EOSManager.AuthInterface.CopyUserAuthToken(ref copyUserAuthTokenOptions, accountId, out var authToken);

        if (result == Result.Success && authToken.HasValue)
        {
            var authData = new AuthData { RefreshToken = authToken.Value.RefreshToken };
            string json = JsonSerializer.Serialize(authData, DataSaver.SerializerOptions);
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            string fullPath = PersistentData.GetPath(AuthFileName);
            File.WriteAllText(fullPath, base64);
        }
        else
        {
            FusionLogger.Error($"Failed to get user auth token for saving: {result}");
        }
    }

    private static AuthData LoadAuthData()
    {
        string fullPath = PersistentData.GetPath(AuthFileName);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        try
        {
            string base64 = File.ReadAllText(fullPath);
            if (string.IsNullOrEmpty(base64))
            {
                return null;
            }

            byte[] bytes = Convert.FromBase64String(base64);
            string json = Encoding.UTF8.GetString(bytes);
            return DataSaver.ReadJsonFromText<AuthData>(json);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("loading encoded auth data", e);
            ClearAuthData(); // Corrupted file, delete it
            return null;
        }
    }

    private static void ClearAuthData()
    {
        string path = PersistentData.GetPath(AuthFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    internal static IEnumerator Login(System.Action<bool> onComplete)
    {
        // Try to login with a saved refresh token
        TaskCompletionSource<bool> refreshTokenAttempt = new();
        var savedAuthData = LoadAuthData();

        if (File.Exists(PersistentData.GetPath("NOAUTHFILE.txt")))
            savedAuthData = null;

        if (savedAuthData != null && !string.IsNullOrEmpty(savedAuthData.RefreshToken))
            {
                var refreshTokenOptions = new Epic.OnlineServices.Auth.LoginOptions()
                {
                    Credentials = new Epic.OnlineServices.Auth.Credentials()
                    {
                        Type = LoginCredentialType.RefreshToken,
                        Token = savedAuthData.RefreshToken
                    },
                    ScopeFlags = AuthScopeFlags.BasicProfile |
                                 AuthScopeFlags.Presence |
                                 AuthScopeFlags.FriendsList |
                                 AuthScopeFlags.Country,
                };
                EOSManager.AuthInterface.Login(ref refreshTokenOptions, null, (ref Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo) =>
                {
                    if (loginCallbackInfo.ResultCode == Result.Success)
                    {
                        EOSNetworkLayer.LocalAccountId = loginCallbackInfo.LocalUserId;
                        refreshTokenAttempt.SetResult(true);
                    }
                    else
                    {
                        FusionLogger.Warn($"Failed to login with saved refresh token ({loginCallbackInfo.ResultCode}), clearing saved data.");
                        ClearAuthData();
                        refreshTokenAttempt.SetResult(false);
                    }
                });

                while (!refreshTokenAttempt.Task.IsCompleted)
                    yield return null;

                if (refreshTokenAttempt.Task.Result)
                {
                    onComplete?.Invoke(true);
                    yield break;
                }
            }

        // If there is no saved data or the refresh token failed, use the account portal
        TaskCompletionSource<bool> portalLoginAttempt = new TaskCompletionSource<bool>();
        var portalLoginOptions = new Epic.OnlineServices.Auth.LoginOptions()
        {
            Credentials = new Epic.OnlineServices.Auth.Credentials()
            {
                Type = LoginCredentialType.AccountPortal,
                Id = null,
                Token = null
            },
            ScopeFlags = AuthScopeFlags.BasicProfile |
                         AuthScopeFlags.Presence |
                         AuthScopeFlags.FriendsList |
                         AuthScopeFlags.Country,
        };
        EOSManager.AuthInterface.Login(ref portalLoginOptions, null, (ref Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo) =>
        {
            FusionLogger.Log(loginCallbackInfo.PinGrantInfo?.VerificationURI);
            // If account portal login succeeds, save the auth data
            if (loginCallbackInfo.ResultCode == Result.Success)
            {
                EOSNetworkLayer.LocalAccountId = loginCallbackInfo.LocalUserId;

                if (!File.Exists(PersistentData.GetPath("NOAUTHFILE.txt")))
                    SaveAuthData(loginCallbackInfo.LocalUserId);
                    
                portalLoginAttempt.SetResult(true);
            }
            else
            {
                portalLoginAttempt.SetResult(false);
            }
        });

        while (!portalLoginAttempt.Task.IsCompleted)
            yield return null;

        if (portalLoginAttempt.Task.Result == true)
        {
            onComplete?.Invoke(true);
            yield break;
        }

        FusionLogger.Error("Failed to login to EOS.");
        onComplete?.Invoke(false);
    }

    internal static IEnumerator SetupConnectLogin(System.Action<bool> onComplete)
    {
        var copyIdTokenOptions = new Epic.OnlineServices.Auth.CopyIdTokenOptions
        {
            AccountId = EOSNetworkLayer.LocalAccountId,
        };
        var idTokenResult = EOSManager.AuthInterface.CopyIdToken(ref copyIdTokenOptions, out var idToken);

        if (idTokenResult != Result.Success)
        {
            FusionLogger.Error($"Failed to get Auth ID token for Connect login: {idTokenResult}");
            onComplete?.Invoke(false);
            yield break;
        }

        var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions
        {
            Credentials = new Epic.OnlineServices.Connect.Credentials
            {
                Type = ExternalCredentialType.EpicIdToken,
                Token = idToken.Value.JsonWebToken,
            },
        };

        bool loginComplete = false;
        bool loginSuccess = false;
        EOSManager.ConnectInterface.Login(ref connectLoginOptions, null, (ref Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo) =>
        {
            if (connectLoginCallbackInfo.ResultCode == Result.Success)
            {
                loginSuccess = true;
                EOSNetworkLayer.LocalUserId = connectLoginCallbackInfo.LocalUserId;
                loginComplete = true;
            }
            else if (connectLoginCallbackInfo.ResultCode == Result.InvalidUser && connectLoginCallbackInfo.ContinuanceToken != null)
            {
                var createUserOptions = new CreateUserOptions
                {
                    ContinuanceToken = connectLoginCallbackInfo.ContinuanceToken
                };

                EOSManager.ConnectInterface.CreateUser(ref createUserOptions, null, (ref CreateUserCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode != Result.Success)
                    {
                        FusionLogger.Error($"Failed to create new user: {callbackInfo.ResultCode}");
                        loginComplete = true;
                        return;
                    }

                    EOSNetworkLayer.LocalUserId = callbackInfo.LocalUserId;
#if DEBUG
                    FusionLogger.Log($"New user created successfully. ProductUserId: {EOSNetworkLayer.LocalUserId}");
#endif
                    loginSuccess = true;
                    loginComplete = true;
                });
            }
            else
            {
                FusionLogger.Error($"Connect login failed with result: {connectLoginCallbackInfo.ResultCode}");
                loginComplete = true;
            }
        });

        while (!loginComplete)
            yield return null;

        if (!loginSuccess)
        {
            FusionLogger.Error($"Connect login failed!");
            onComplete?.Invoke(false);
            yield break;
        }

        onComplete?.Invoke(true);
    }
}