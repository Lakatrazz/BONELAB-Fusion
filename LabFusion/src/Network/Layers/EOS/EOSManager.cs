using Epic.OnlineServices;

using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static LabFusion.Network.EOSNetworkLayer;

namespace LabFusion.Network
{
	internal class EOSManager
	{
		// EOS Properties
		static string ProductName => "BONELAB Fusion";
		static string ProductVersion => "1.0";
		static string ProductId => "29e074d5b4724f3bb01f26b7e33d2582";
		static string SandboxId => "26f32d66d87f4dfeb4a7449b776a41f1";
		static string DeploymentId => "1dffb21201e04ad89b0e6e415f0b8993";
		static string ClientId => "xyza7891gWLwVJx3rdLOLs6vJ05u9jWT";
		static string ClientSecret => "IWrUy1Z62wWajAX37k3zkQ4Kkto+AvfQSyZ9zfvibzw";

		internal static void InitEOS()
		{
			Epic.OnlineServices.Logging.LoggingInterface.SetLogLevel(Epic.OnlineServices.Logging.LogCategory.AllCategories, LogLevel);
			Epic.OnlineServices.Logging.LoggingInterface.SetCallback((ref Epic.OnlineServices.Logging.LogMessage logMessage) =>
			{
				// https://eoshelp.epicgames.com/s/article/Why-is-the-warning-LogEOS-FEpicGamesPlatform-GetOnlinePlatformType-unable-to-map-None-to-EOS-OnlinePlatformType-thrown?language=en_US
				if (logMessage.Message == "FEpicGamesPlatform::GetOnlinePlatformType - unable to map None to EOS_OnlinePlatformType")
					return;

				FusionLogger.Log(logMessage.Message);
			});

			EOSJNI.EOS_Init();

            var initializeOptions = new Epic.OnlineServices.Platform.AndroidInitializeOptions()
			{
				ProductName = ProductName,
				ProductVersion = ProductVersion,
			};
			var initializeResult = Epic.OnlineServices.Platform.PlatformInterface.Initialize(ref initializeOptions);

			if (initializeResult != Result.Success)
			{
				FusionLogger.Error($"Failed to initialize EOS Platform: {initializeResult}");
				return;
            }

            var options = new Epic.OnlineServices.Platform.Options()
			{
				ProductId = ProductId,
				SandboxId = SandboxId,
				DeploymentId = DeploymentId,
				ClientCredentials = new Epic.OnlineServices.Platform.ClientCredentials()
				{
					ClientId = ClientId,
					ClientSecret = ClientSecret
				},
			};
			PlatformInterface = Epic.OnlineServices.Platform.PlatformInterface.Create(ref options);
			if (PlatformInterface == null)
			{
				FusionLogger.Error("Failed to create EOS Platform Interface");
				return;
            }

			AuthInterface = PlatformInterface.GetAuthInterface();
			ConnectInterface = PlatformInterface.GetConnectInterface();
			P2PInterface = PlatformInterface.GetP2PInterface();
			LobbyInterface = PlatformInterface.GetLobbyInterface();
			FriendsInterface = PlatformInterface.GetFriendsInterface();

            // Login with persistent auth
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

		internal static void ShutdownEOS()
		{
			PlatformInterface.Release();
			PlatformInterface = null;
        }

		private static void OnAuthLoginComplete(ref Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo)
		{
			// If persistent auth fails, use EOS account portal
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
			// Finally, exit if portal login fails
			if (portalLoginCallbackInfo.ResultCode == Result.Success)
			{
				FusionLogger.Log("Auth Login succeeded via account portal.");
				LocalAccountId = portalLoginCallbackInfo.LocalUserId;
				SetupConnectLogin();
			}
			else
			{
				FusionLogger.Error("All login attempts failed: " + portalLoginCallbackInfo.ResultCode);
				ShutdownEOS();
            }
		}

		private static void SetupConnectLogin()
		{
            var copyIdTokenOptions = new Epic.OnlineServices.Auth.CopyIdTokenOptions { AccountId = LocalAccountId };
            var idTokenResult = AuthInterface.CopyIdToken(ref copyIdTokenOptions, out var idToken);

            if (idTokenResult == Result.Success)
            {
                var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions
                {
                    Credentials = new Epic.OnlineServices.Connect.Credentials
                    {
                        Type = ExternalCredentialType.EpicIdToken,
                        Token = idToken.Value.JsonWebToken,
                    },
                };

                ConnectInterface.Login(ref connectLoginOptions, null, OnConnectLoginComplete);
            }
            else
            {
                FusionLogger.Error($"Failed to get Auth ID token for Connect login: {idTokenResult}");
            }
        }

		private static void OnConnectLoginComplete(ref Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo)
		{
			var layer = NetworkLayerManager.Layer as EOSNetworkLayer;

			if (connectLoginCallbackInfo.ResultCode == Result.Success)
			{
				LocalUserId = connectLoginCallbackInfo.LocalUserId;

#if DEBUG
				FusionLogger.Log($"Connect login successful. ProductUserId: {LocalUserId}");
#endif
				// Setup Fusion local player data
				PlayerIDManager.SetStringID(LocalUserId.ToString());
				LocalPlayer.Username = layer.GetUsername(LocalAccountId.ToString());

				EOSSocketHandler.ConfigureP2P();
			}
			else if (connectLoginCallbackInfo.ResultCode == Result.InvalidUser && connectLoginCallbackInfo.ContinuanceToken != null)
			{
#if DEBUG
				FusionLogger.Log("New user needs to be created with continuance token");
#endif

				var createUserOptions = new Epic.OnlineServices.Connect.CreateUserOptions
                {
                    ContinuanceToken = connectLoginCallbackInfo.ContinuanceToken
                };

                ConnectInterface.CreateUser(ref createUserOptions, null, OnCreateUserComplete);
            }
			else
			{
				FusionLogger.Error($"Connect login failed: {connectLoginCallbackInfo.ResultCode}");
			}
		}

		private static void OnCreateUserComplete(ref Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo)
		{
			var layer = NetworkLayerManager.Layer as EOSNetworkLayer;

			if (createUserCallbackInfo.ResultCode == Result.Success)
			{
				LocalUserId = createUserCallbackInfo.LocalUserId;

#if DEBUG
				FusionLogger.Log($"New user created successfully. ProductUserId: {LocalUserId}");
#endif

				PlayerIDManager.SetStringID(LocalUserId.ToString());
				LocalPlayer.Username = layer.GetUsername(LocalAccountId.ToString());

				EOSSocketHandler.ConfigureP2P();
			}
			else
			{
				FusionLogger.Error($"Failed to create new user: {createUserCallbackInfo.ResultCode}");
			}
		}

		public static EpicAccountId GetEpicAccountIdFromProductUserId(ProductUserId productUserId)
		{
			if (productUserId == null || ConnectInterface == null)
				return null;

			var options = new Epic.OnlineServices.Connect.CopyProductUserExternalAccountByAccountTypeOptions
			{
				TargetUserId = productUserId,
				AccountIdType = ExternalAccountType.Epic,
			};

			Result result = ConnectInterface.CopyProductUserExternalAccountByAccountType(ref options, out Epic.OnlineServices.Connect.ExternalAccountInfo? externalAccountInfo);

			if (result == Result.Success && externalAccountInfo.HasValue)
			{
				return EpicAccountId.FromString(externalAccountInfo.Value.AccountId);
			}
			else if (result != Result.Success)
			{
				FusionLogger.Warn($"Failed to get EpicAccountId for ProductUserId {productUserId}: {result}");
				return null;
			}

			return null;
		}
	}
}
