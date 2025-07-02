using Epic.OnlineServices;
using LabFusion.Player;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static LabFusion.Network.EOSNetworkLayer;

namespace LabFusion.Network
{
    internal class EOSAuthenticator
    {
        // EOS Properties
        static string ProductName => "BONELAB Fusion";
        static string ProductVersion => "1.0";
        static string ProductId => "29e074d5b4724f3bb01f26b7e33d2582";
        static string SandboxId => "26f32d66d87f4dfeb4a7449b776a41f1";
        static string DeploymentId => "1dffb21201e04ad89b0e6e415f0b8993";
        static string ClientId => "xyza7891gWLwVJx3rdLOLs6vJ05u9jWT";
        static string ClientSecret => "IWrUy1Z62wWajAX37k3zkQ4Kkto+AvfQSyZ9zfvibzw";

        internal static void InitializeEOS()
        {
            var initializeOptions = new Epic.OnlineServices.Platform.InitializeOptions()
            {
                ProductName = ProductName,
                ProductVersion = ProductVersion
            };

            var initializeResult = Epic.OnlineServices.Platform.PlatformInterface.Initialize(ref initializeOptions);
            if (initializeResult != Result.Success)
            {
                throw new Exception("Failed to initialize platform: " + initializeResult);
            }

            Epic.OnlineServices.Logging.LoggingInterface.SetLogLevel(Epic.OnlineServices.Logging.LogCategory.AllCategories, LogLevel);
            Epic.OnlineServices.Logging.LoggingInterface.SetCallback((ref Epic.OnlineServices.Logging.LogMessage logMessage) =>
            {
                // https://eoshelp.epicgames.com/s/article/Why-is-the-warning-LogEOS-FEpicGamesPlatform-GetOnlinePlatformType-unable-to-map-None-to-EOS-OnlinePlatformType-thrown?language=en_US
                if (logMessage.Message == "FEpicGamesPlatform::GetOnlinePlatformType - unable to map None to EOS_OnlinePlatformType")
                    return;

                FusionLogger.Log(logMessage.Message);
            });


            var options = new Epic.OnlineServices.Platform.Options()
            {
                ProductId = ProductId,
                SandboxId = SandboxId,
                DeploymentId = DeploymentId,
                ClientCredentials = new Epic.OnlineServices.Platform.ClientCredentials()
                {
                    ClientId = ClientId,
                    ClientSecret = ClientSecret
                }
            };

            PlatformInterface = Epic.OnlineServices.Platform.PlatformInterface.Create(ref options);
            if (PlatformInterface == null)
            {
                throw new Exception("Failed to create platform");
            }

            AuthInterface = PlatformInterface.GetAuthInterface();
            if (AuthInterface == null)
            {
                throw new Exception("Failed to get auth interface");
            }

            var authLoginOptions = new Epic.OnlineServices.Auth.LoginOptions()
            {
                Credentials = new Epic.OnlineServices.Auth.Credentials()
                {
                    Type = Epic.OnlineServices.Auth.LoginCredentialType.PersistentAuth,
                    Id = null,
                    Token = null
                },
                ScopeFlags = Epic.OnlineServices.Auth.AuthScopeFlags.BasicProfile |
                             Epic.OnlineServices.Auth.AuthScopeFlags.Presence |
                             Epic.OnlineServices.Auth.AuthScopeFlags.FriendsList |
                             Epic.OnlineServices.Auth.AuthScopeFlags.Country
            };

            AuthInterface.Login(ref authLoginOptions, null, OnAuthLoginComplete);
        }

        private static void OnAuthLoginComplete(ref Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo)
        {
            if (loginCallbackInfo.ResultCode == Result.Success)
            {
                LocalAccountId = loginCallbackInfo.LocalUserId;
                SetupConnectLogin();
            }
            else
            {
                var portalLoginOptions = new Epic.OnlineServices.Auth.LoginOptions()
                {
                    Credentials = new Epic.OnlineServices.Auth.Credentials()
                    {
                        Type = Epic.OnlineServices.Auth.LoginCredentialType.AccountPortal,
                        Id = null,
                        Token = null
                    },
                    ScopeFlags = Epic.OnlineServices.Auth.AuthScopeFlags.BasicProfile |
                                 Epic.OnlineServices.Auth.AuthScopeFlags.Presence |
                                 Epic.OnlineServices.Auth.AuthScopeFlags.FriendsList |
                                 Epic.OnlineServices.Auth.AuthScopeFlags.Country
                };

                AuthInterface.Login(ref portalLoginOptions, null, OnPortalLoginComplete);
            }
        }

        private static void OnPortalLoginComplete(ref Epic.OnlineServices.Auth.LoginCallbackInfo portalLoginCallbackInfo)
        {
            if (portalLoginCallbackInfo.ResultCode == Result.Success)
            {
                FusionLogger.Log("Auth Login succeeded via account portal.");
                LocalAccountId = portalLoginCallbackInfo.LocalUserId;
                SetupConnectLogin();
            }
            else
            {
                FusionLogger.Error("All login attempts failed: " + portalLoginCallbackInfo.ResultCode);
            }
        }

        private static void SetupConnectLogin()
        {
            ConnectInterface = PlatformInterface.GetConnectInterface();

            if (LocalAccountId != null && ConnectInterface != null)
            {
                var copyIdTokenOptions = new Epic.OnlineServices.Auth.CopyIdTokenOptions { AccountId = LocalAccountId };
                var idTokenResult = AuthInterface.CopyIdToken(ref copyIdTokenOptions, out var idToken);

                if (idTokenResult == Result.Success && idToken != null)
                {
                    FusionLogger.Log($"Using Auth ID Token for Connect Login");

                    var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions
                    {
                        Credentials = new Epic.OnlineServices.Connect.Credentials
                        {
                            Type = Epic.OnlineServices.ExternalCredentialType.EpicIdToken,
                            Token = idToken.Value.JsonWebToken
                        },
                    };

                    ConnectInterface.Login(ref connectLoginOptions, null, OnConnectLoginComplete);
                }
                else
                {
                    FusionLogger.Error($"Failed to get Auth ID token for Connect login: {idTokenResult}");
                }
            }
            else
            {
                FusionLogger.Error("LocalAccountId or ConnectInterface is null after Auth login.");
            }
        }

        private static void OnConnectLoginComplete(ref Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo)
        {
            if (connectLoginCallbackInfo.ResultCode == Result.Success)
            {
                LocalUserId = connectLoginCallbackInfo.LocalUserId;
                FusionLogger.Log($"Connect login successful. ProductUserId: {LocalUserId}");
                PlayerIDManager.SetStringID(LocalUserId.ToString());

                ConfigureP2P();
            }
            else if (connectLoginCallbackInfo.ResultCode == Result.InvalidUser)
            {
                if (connectLoginCallbackInfo.ContinuanceToken != null)
                {
                    FusionLogger.Log("New user needs to be created with continuance token");

                    var createUserOptions = new Epic.OnlineServices.Connect.CreateUserOptions
                    {
                        ContinuanceToken = connectLoginCallbackInfo.ContinuanceToken
                    };

                    ConnectInterface.CreateUser(ref createUserOptions, null, OnCreateUserComplete);
                }
            }
            else
            {
                FusionLogger.Error($"Connect login failed: {connectLoginCallbackInfo.ResultCode}");
            }
        }

        private static void OnCreateUserComplete(ref Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo)
        {
            if (createUserCallbackInfo.ResultCode == Result.Success)
            {
                LocalUserId = createUserCallbackInfo.LocalUserId;
                FusionLogger.Log($"New user created successfully. ProductUserId: {LocalUserId}");
                PlayerIDManager.SetStringID(LocalUserId.ToString());

                ConfigureP2P();
            }
            else
            {
                FusionLogger.Error($"Failed to create new user: {createUserCallbackInfo.ResultCode}");
            }
        }

        private static void ConfigureP2P()
        {
            LoadUsername(LocalUserId.ToString());

            EOSSocketHandler.ConfigureP2PSocketToAcceptConnections();
        }

        private static void LoadUsername(string userID)
        {
            if (PlatformInterface == null || LocalAccountId == null)
            {
                LocalPlayer.Username = "Unknown";
            }

            var userInfoInterface = PlatformInterface.GetUserInfoInterface();
            if (userInfoInterface != null)
            {
                var userInfoOptions = new Epic.OnlineServices.UserInfo.QueryUserInfoOptions
                {
                    LocalUserId = LocalAccountId,
                    TargetUserId = LocalAccountId
                };

                userInfoInterface.QueryUserInfo(ref userInfoOptions, null, (ref Epic.OnlineServices.UserInfo.QueryUserInfoCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode == Result.Success)
                    {
                        var copyOptions = new Epic.OnlineServices.UserInfo.CopyUserInfoOptions
                        {
                            LocalUserId = LocalAccountId,
                            TargetUserId = LocalAccountId
                        };

                        if (userInfoInterface.CopyUserInfo(ref copyOptions, out var userInfo) == Result.Success)
                        {
                            LocalPlayer.Username = userInfo?.DisplayName ?? "EOS User";
                        }
                    }
                });
            }
        }
    }
}
