using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;

using Il2CppSystem;
using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Il2Cpp.Interop;
using static Il2CppTrees.SparseVoxelOctree;
using static LabFusion.Network.EOSNetworkLayer;

namespace LabFusion.Network
{
	internal class EOSManager
	{
		public static PlatformInterface PlatformInterface;
		public static AuthInterface AuthInterface;
		public static ConnectInterface ConnectInterface;
		public static P2PInterface P2PInterface;
		public static LobbyInterface LobbyInterface;
		public static FriendsInterface FriendsInterface;

		public static string SavedUsername = string.Empty;

		// EOS Properties
		protected static string ProductName => "BONELAB Fusion";
		protected static string ProductVersion => "1.0";
		protected static string ProductId => "29e074d5b4724f3bb01f26b7e33d2582";
		protected static string SandboxId => "26f32d66d87f4dfeb4a7449b776a41f1";
		protected static string DeploymentId => "1dffb21201e04ad89b0e6e415f0b8993";
		protected static string ClientId => "xyza7891gWLwVJx3rdLOLs6vJ05u9jWT";
		protected static string ClientSecret => "IWrUy1Z62wWajAX37k3zkQ4Kkto+AvfQSyZ9zfvibzw";

		private static IEnumerator Ticker()
		{
			float timePassed = 0f;
			while (PlatformInterface != null)
			{
				timePassed += TimeUtilities.DeltaTime;
				if (timePassed >= 1f/20f)
				{
					timePassed = 0f;
					PlatformInterface?.Tick();
				}
				yield return null;
			}

			yield break;
		}

		internal static IEnumerator InitEOS(System.Action<bool> onComplete)
		{
			// Setup debug logging
			Epic.OnlineServices.Logging.LoggingInterface.SetLogLevel(Epic.OnlineServices.Logging.LogCategory.AllCategories, LogLevel);
			Epic.OnlineServices.Logging.LoggingInterface.SetCallback((ref Epic.OnlineServices.Logging.LogMessage logMessage) =>
			{
				// https://eoshelp.epicgames.com/s/article/Why-is-the-warning-LogEOS-FEpicGamesPlatform-GetOnlinePlatformType-unable-to-map-None-to-EOS-OnlinePlatformType-thrown?language=en_US
				if (logMessage.Message == "FEpicGamesPlatform::GetOnlinePlatformType - unable to map None to EOS_OnlinePlatformType")
					return;

				FusionLogger.Log(logMessage.Message);
			});

			// Android specific initialization
			if (PlatformHelper.IsAndroid)
				EOSJNI.EOS_Init();

			if (!InitializeInterfaces())
			{
				onComplete?.Invoke(false);
				yield break;
			}

			MelonCoroutines.Start(Ticker());

			bool loginComplete = false;
			bool loginSuccess = false;
			MelonCoroutines.Start(Login((success) =>
			{
				loginSuccess = success;
				loginComplete = true;
			}));

			while (!loginComplete)
				yield return null;

			if (!loginSuccess)
			{
				ShutdownEOS();
				onComplete?.Invoke(false);
				yield break;
			}

			bool connectComplete = false;
			bool connectSuccess = false;
			MelonCoroutines.Start(SetupConnectLogin((success) =>
			{
				connectSuccess = success;
				connectComplete = true;
			}));

			while (!connectComplete)
				yield return null;

			if (!connectSuccess)
			{
				ShutdownEOS();
				onComplete?.Invoke(false);
				yield break;
			}

			bool usernameComplete = false;
			MelonCoroutines.Start(SetupUsername(LocalAccountId, (username) =>
			{
				usernameComplete = true;
				SavedUsername = username;
			}));

			while (!usernameComplete)
				yield return null;

			EOSSocketHandler.ConfigureP2P();

			onComplete.Invoke(true);
			yield break;
		}

		private static bool InitializeInterfaces()
		{
			var initializeOptions = new InitializeOptions()
			{
				ProductName = ProductName,
				ProductVersion = ProductVersion,
			};
			var initializeResult = PlatformInterface.Initialize(ref initializeOptions);

			if (initializeResult != Result.Success && initializeResult != Result.AlreadyConfigured)
			{
				FusionLogger.Error($"Failed to initialize EOS Platform: {initializeResult}");
				return false;
			}

			var options = new Options()
			{
				ProductId = ProductId,
				SandboxId = SandboxId,
				DeploymentId = DeploymentId,
				ClientCredentials = new ClientCredentials()
				{
					ClientId = ClientId,
					ClientSecret = ClientSecret
				},
			};
			PlatformInterface = PlatformInterface.Create(ref options);
			if (PlatformInterface == null)
			{
				FusionLogger.Error("Failed to create EOS Platform Interface");
				return false;
			}

			AuthInterface = PlatformInterface.GetAuthInterface();
			ConnectInterface = PlatformInterface.GetConnectInterface();
			P2PInterface = PlatformInterface.GetP2PInterface();
			LobbyInterface = PlatformInterface.GetLobbyInterface();
			FriendsInterface = PlatformInterface.GetFriendsInterface();

			return true;
		}

		private static IEnumerator Login(System.Action<bool> onComplete)
		{
			// First attempt persistent auth login
			TaskCompletionSource<bool> persistentLoginAttempt = new();
			var persistentLoginOptions = new Epic.OnlineServices.Auth.LoginOptions()
			{
				Credentials = new Epic.OnlineServices.Auth.Credentials()
				{
					Type = LoginCredentialType.PersistentAuth,
					Id = null,
					Token = null
				},
				ScopeFlags = AuthScopeFlags.BasicProfile |
							 AuthScopeFlags.Presence |
							 AuthScopeFlags.FriendsList |
							 AuthScopeFlags.Country
			};
			AuthInterface.Login(ref persistentLoginOptions, null, (ref Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo) =>
			{
				// If persistent auth fails, use EOS account portal
				if (loginCallbackInfo.ResultCode == Result.Success)
				{
					LocalAccountId = loginCallbackInfo.LocalUserId;
					persistentLoginAttempt.SetResult(true);
				}
				else
				{
					persistentLoginAttempt.SetResult(false);
				}
			});

			while (!persistentLoginAttempt.Task.IsCompleted)
				yield return null;

			if (persistentLoginAttempt.Task.Result == true)
			{
				onComplete?.Invoke(true);
				yield break;
			}

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
							 AuthScopeFlags.Country
			};
			AuthInterface.Login(ref portalLoginOptions, null, (ref Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo) =>
			{
				// If persistent auth fails, use EOS account portal
				if (loginCallbackInfo.ResultCode == Result.Success)
				{
					LocalAccountId = loginCallbackInfo.LocalUserId;
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

		internal static void ShutdownEOS()
		{
			PlatformInterface?.Release();
			PlatformInterface = null;
			AuthInterface = null;
			ConnectInterface = null;
			P2PInterface = null;
			LobbyInterface = null;
			FriendsInterface = null;
		}

		private static IEnumerator SetupConnectLogin(System.Action<bool> onComplete)
		{
			var copyIdTokenOptions = new Epic.OnlineServices.Auth.CopyIdTokenOptions
			{ 
				AccountId = LocalAccountId,
			};
			var idTokenResult = AuthInterface.CopyIdToken(ref copyIdTokenOptions, out var idToken);

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
			ConnectInterface.Login(ref connectLoginOptions, null, (ref Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo) =>
			{
				if (connectLoginCallbackInfo.ResultCode == Result.Success)
				{
					loginSuccess = true;
					LocalUserId = connectLoginCallbackInfo.LocalUserId;
					loginComplete = true;
				} 
				else if (connectLoginCallbackInfo.ResultCode == Result.InvalidUser && connectLoginCallbackInfo.ContinuanceToken != null)
				{
					var createUserOptions = new CreateUserOptions
					{
						ContinuanceToken = connectLoginCallbackInfo.ContinuanceToken
					};

					ConnectInterface.CreateUser(ref createUserOptions, null, (ref CreateUserCallbackInfo callbackInfo) =>
					{
						if (callbackInfo.ResultCode != Result.Success)
						{
							FusionLogger.Error($"Failed to create new user: {callbackInfo.ResultCode}");
							loginComplete = true;
							return;
						}

						LocalUserId = callbackInfo.LocalUserId;
#if DEBUG
						FusionLogger.Log($"New user created successfully. ProductUserId: {LocalUserId}");
#endif
					} );
				} 
				else
				{
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

		public static IEnumerator SetupUsername(EpicAccountId accountId, System.Action<string> onComplete)
		{
			var userInfoInterface = PlatformInterface.GetUserInfoInterface();
			var userInfoOptions = new Epic.OnlineServices.UserInfo.QueryUserInfoOptions
			{
				LocalUserId = LocalAccountId,
				TargetUserId = accountId
			};

			TaskCompletionSource<string> usernameTask = new TaskCompletionSource<string>();
			userInfoInterface.QueryUserInfo(ref userInfoOptions, null, (ref Epic.OnlineServices.UserInfo.QueryUserInfoCallbackInfo callbackInfo) =>
			{
				if (callbackInfo.ResultCode != Result.Success)
				{
					usernameTask.SetResult(string.Empty);
					return;
				}

				var copyOptions = new Epic.OnlineServices.UserInfo.CopyUserInfoOptions
				{
					LocalUserId = LocalAccountId,
					TargetUserId = accountId
				};

				if (userInfoInterface.CopyUserInfo(ref copyOptions, out var userInfo) == Result.Success)
					usernameTask.SetResult(userInfo.Value.DisplayName ?? "Unknown");
			});

			while (!usernameTask.Task.IsCompleted)
				yield return null;

			onComplete?.Invoke(usernameTask.Task.Result);
			yield break;
		}

		public static EpicAccountId GetAccountIdFromProductId(ProductUserId productUserId)
		{
			if (productUserId == null)
				return null;

			var options = new CopyProductUserExternalAccountByAccountTypeOptions
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
			}

			return null;
		}
	}
}
