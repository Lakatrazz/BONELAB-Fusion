using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Il2CppOculus.Platform;
using Il2CppOculus.Platform.Models;
using LabFusion.Data;
using LabFusion.Utilities;
using MelonLoader;
using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace LabFusion.Network;

internal class EOSAuthAndroid
{
	[System.Serializable]
	private class AuthData
	{
		[JsonPropertyName("refreshToken")]
		public string RefreshToken { get; set; }
	}

	private const string AuthFileName = "eos_auth_android.dat";

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
		FusionLogger.Log("Android Auth");

		// Try to login with a saved refresh token first
		TaskCompletionSource<bool> refreshTokenAttempt = new();
		var savedAuthData = LoadAuthData();

		if (savedAuthData != null && !string.IsNullOrEmpty(savedAuthData.RefreshToken))
		{
			var refreshTokenOptions = new Epic.OnlineServices.Auth.LoginOptions()
			{
				Credentials = new Epic.OnlineServices.Auth.Credentials()
				{
					Type = LoginCredentialType.RefreshToken,
					Token = savedAuthData.RefreshToken,
					Id = null
				},
				ScopeFlags = AuthScopeFlags.BasicProfile |
							 AuthScopeFlags.Presence |
							 AuthScopeFlags.FriendsList |
							 AuthScopeFlags.Country
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

		// Try AccountPortal (modern replacement for DeviceCode)
		FusionLogger.Log("Attempting AccountPortal authentication (device code flow)...");

		TaskCompletionSource<bool> accountPortalAttempt = new();

		var accountPortalOptions = new Epic.OnlineServices.Auth.LoginOptions()
		{
			Credentials = new Epic.OnlineServices.Auth.Credentials()
			{
				Type = LoginCredentialType.AccountPortal,
				Id = null,
				Token = null
			},
			LoginFlags = LoginFlags.NoUserInterface,
			ScopeFlags = AuthScopeFlags.BasicProfile |
						 AuthScopeFlags.Presence |
						 AuthScopeFlags.FriendsList |
						 AuthScopeFlags.Country
		};

		bool accountPortalComplete = false;
		bool accountPortalSuccess = false;
		PinGrantInfo? pinGrantInfo = null;

		EOSManager.AuthInterface.Login(ref accountPortalOptions, null, (ref Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo) =>
		{
			FusionLogger.Log($"Enter code: {pinGrantInfo?.VerificationURI}");

			if (loginCallbackInfo.ResultCode == Result.Success)
			{
				EOSNetworkLayer.LocalAccountId = loginCallbackInfo.LocalUserId;
				SaveAuthData(loginCallbackInfo.LocalUserId);
				accountPortalSuccess = true;
				accountPortalComplete = true;
				FusionLogger.Log("AccountPortal authentication successful!");
			}
			else if (loginCallbackInfo.ResultCode == Result.AuthPinGrantCode && loginCallbackInfo.PinGrantInfo.HasValue)
			{
				// This is the device code flow!
				pinGrantInfo = loginCallbackInfo.PinGrantInfo.Value;

				FusionLogger.Log("=== DEVICE CODE AUTHENTICATION ===");
				FusionLogger.Log($"Go to: {pinGrantInfo.Value.VerificationURI}");
				FusionLogger.Log($"Enter code: {pinGrantInfo.Value.UserCode}");
				FusionLogger.Log($"Or visit: {pinGrantInfo.Value.VerificationURIComplete}");
				FusionLogger.Log($"You have {pinGrantInfo.Value.ExpiresIn} seconds to complete this process");
				FusionLogger.Log("==================================");

				// Don't complete yet - we need to start polling
			}
			else
			{
				FusionLogger.Error($"AccountPortal authentication failed: {loginCallbackInfo.ResultCode}");
				accountPortalComplete = true;
			}
		});

		// Wait for initial response
		while (!accountPortalComplete && pinGrantInfo == null)
			yield return null;

		// If we got a pin grant code, start polling (device code flow)
		if (pinGrantInfo.HasValue)
		{
			FusionLogger.Log("Waiting for user to complete authentication on their device...");

			float pollInterval = 5.0f;
			float timeElapsed = 0.0f;
			float timeout = pinGrantInfo.Value.ExpiresIn;

			while (timeElapsed < timeout && !accountPortalComplete)
			{
				yield return new WaitForSeconds(pollInterval);
				timeElapsed += pollInterval;

				// Poll by calling login again
				EOSManager.AuthInterface.Login(ref accountPortalOptions, null, (ref Epic.OnlineServices.Auth.LoginCallbackInfo pollCallbackInfo) =>
				{
					if (pollCallbackInfo.ResultCode == Result.Success)
					{
						EOSNetworkLayer.LocalAccountId = pollCallbackInfo.LocalUserId;
						SaveAuthData(pollCallbackInfo.LocalUserId);
						accountPortalSuccess = true;
						accountPortalComplete = true;
						FusionLogger.Log("Device code authentication completed successfully!");
					}
					else if (pollCallbackInfo.ResultCode == Result.AuthPinGrantPending)
					{
						FusionLogger.Log($"Still waiting for authentication... ({(int)(timeout - timeElapsed)} seconds remaining)");
					}
					else if (pollCallbackInfo.ResultCode == Result.AuthPinGrantExpired)
					{
						FusionLogger.Error("Device code has expired. Please try again.");
						accountPortalComplete = true;
					}
					else
					{
						FusionLogger.Error($"Device code authentication failed: {pollCallbackInfo.ResultCode}");
						accountPortalComplete = true;
					}
				});

				// Wait for poll callback
				while (!accountPortalComplete && timeElapsed < timeout)
				{
					yield return new WaitForSeconds(0.1f);
					timeElapsed += 0.1f;
				}
			}

			if (timeElapsed >= timeout && !accountPortalSuccess)
			{
				FusionLogger.Error("Device code authentication timed out.");
			}
		}

		if (accountPortalSuccess)
		{
			onComplete?.Invoke(true);
			yield break;
		}

		// Fall back to Oculus if AccountPortal fails
		FusionLogger.Log("Falling back to Oculus authentication...");
		// ... (your existing Oculus auth code)

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