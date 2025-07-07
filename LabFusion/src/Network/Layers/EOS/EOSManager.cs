using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;

using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

namespace LabFusion.Network;

internal class EOSManager
{
	public static PlatformInterface PlatformInterface;
	public static AuthInterface AuthInterface;
	public static ConnectInterface ConnectInterface;
	public static P2PInterface P2PInterface;
	public static LobbyInterface LobbyInterface;
	public static FriendsInterface FriendsInterface;

	public static string SavedUsername = string.Empty;

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
		MelonCoroutines.Start(EOSAuth.Login((success) =>
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
		MelonCoroutines.Start(EOSAuth.SetupConnectLogin((success) =>
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
		MelonCoroutines.Start(SetupUsername(EOSNetworkLayer.LocalAccountId, (username) =>
		{
			usernameComplete = true;
			SavedUsername = username;
		}));

		while (!usernameComplete)
			yield return null;

		EOSSocketHandler.ConfigureP2P();
		EOSInvites.ConfigureInvites();

		onComplete.Invoke(true);
		yield break;
	}

	private static bool InitializeInterfaces()
	{
		var initializeOptions = new InitializeOptions()
		{
			ProductName = EOSAuth.ProductName,
			ProductVersion = EOSAuth.ProductVersion,
		};
		var initializeResult = PlatformInterface.Initialize(ref initializeOptions);

		if (initializeResult != Result.Success && initializeResult != Result.AlreadyConfigured)
		{
			FusionLogger.Error($"Failed to initialize EOS Platform: {initializeResult}");
			return false;
		}

		var options = new Options()
		{
			ProductId = EOSAuth.ProductId,
			SandboxId = EOSAuth.SandboxId,
			DeploymentId = EOSAuth.DeploymentId,
			ClientCredentials = new ClientCredentials()
			{
				ClientId = EOSAuth.ClientId,
				ClientSecret = EOSAuth.ClientSecret
			},
		};
		PlatformInterface = PlatformInterface.Create(ref options);
		if (PlatformInterface == null)
		{
			FusionLogger.Error("Failed to create EOS Platform Interface");
			return false;
		}

		// Setup debug logging
		Epic.OnlineServices.Logging.LoggingInterface.SetLogLevel(Epic.OnlineServices.Logging.LogCategory.AllCategories, EOSNetworkLayer.LogLevel);
		Epic.OnlineServices.Logging.LoggingInterface.SetCallback((ref Epic.OnlineServices.Logging.LogMessage logMessage) =>
		{
			// https://eoshelp.epicgames.com/s/article/Why-is-the-warning-LogEOS-FEpicGamesPlatform-GetOnlinePlatformType-unable-to-map-None-to-EOS-OnlinePlatformType-thrown?language=en_US
			if (logMessage.Message == "FEpicGamesPlatform::GetOnlinePlatformType - unable to map None to EOS_OnlinePlatformType")
				return;

			FusionLogger.Log(logMessage.Message);
		});


		AuthInterface = PlatformInterface.GetAuthInterface();
		ConnectInterface = PlatformInterface.GetConnectInterface();
		P2PInterface = PlatformInterface.GetP2PInterface();
		LobbyInterface = PlatformInterface.GetLobbyInterface();
		FriendsInterface = PlatformInterface.GetFriendsInterface();

		return true;
	}

	internal static void ShutdownEOS()
	{
		EOSInvites.ShutdownInvites();
		PlatformInterface?.Release();
		PlatformInterface = null;
		AuthInterface = null;
		ConnectInterface = null;
		P2PInterface = null;
		LobbyInterface = null;
		FriendsInterface = null;
	}

	public static IEnumerator SetupUsername(EpicAccountId accountId, System.Action<string> onComplete)
	{
		var userInfoInterface = PlatformInterface.GetUserInfoInterface();
		var userInfoOptions = new Epic.OnlineServices.UserInfo.QueryUserInfoOptions
		{
			LocalUserId = EOSNetworkLayer.LocalAccountId,
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
				LocalUserId = EOSNetworkLayer.LocalAccountId,
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
}
