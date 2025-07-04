using Epic.OnlineServices;

using JNISharp.NativeInterface;

using LabFusion.Utilities;

using System.Reflection;
using System.Runtime.InteropServices;

namespace LabFusion.Data;

public static class EOSSDKLoader
{
	public static bool HasEOSSDK { get; private set; } = false;

	private static IntPtr _libraryPtr = IntPtr.Zero;

	private static IntPtr AndroidImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (libraryName == Config.LibraryName + ".so")
			return _libraryPtr;

		return IntPtr.Zero;
	}

	public static async void OnLoadEOSSDK()
	{
		if (HasEOSSDK)
		{
			return;
		}

		string eosSDKPath;
		string libCPPPath;

		if (PlatformHelper.IsAndroid)
		{
			eosSDKPath = PersistentData.GetPath($"libEOSSDK.so");
			libCPPPath = PersistentData.GetPath($"libc++_shared.so");

			if (!File.Exists(eosSDKPath))
			{
				bool eosDownloadSuccess = await DownloadLibraryAsync("libEOSSDK", eosSDKPath);
				if (!eosDownloadSuccess)
				{
					FusionLogger.Error("Failed to download libEOSSDK.so");
					return;
				}
			}

			if (!File.Exists(libCPPPath))
			{
				bool cppDownloadSuccess = await DownloadLibraryAsync("libc++_shared", libCPPPath);
				if (!cppDownloadSuccess)
				{
					FusionLogger.Error("Failed to download libc++_shared.so");
					return;
				}
			}

			if (MelonLoader.NativeLibrary.LoadLib(libCPPPath) == IntPtr.Zero)
				FusionLogger.Error($"Failed to load libc++_shared.so into the application!");
			else
				FusionLogger.Log($"Successfully loaded libc++_shared.so into the application!");
		}
		else
		{
			eosSDKPath = PersistentData.GetPath($"EOSSDK.dll");

			if (!File.Exists(eosSDKPath))
			{
				bool downloadSuccess = await DownloadLibraryAsync("EOSSDK", eosSDKPath);
				if (!downloadSuccess)
				{
					FusionLogger.Error("Failed to download EOSSDK.dll");
					return;
				}
			}
		}

		if (PlatformHelper.IsAndroid)
		{
			InitializeAndroidJNI();
			System.Runtime.InteropServices.NativeLibrary.SetDllImportResolver(typeof(EOSSDKLoader).Assembly, AndroidImportResolver);
		}

		_libraryPtr = MelonLoader.NativeLibrary.LoadLib(eosSDKPath);

		if (_libraryPtr == IntPtr.Zero)
		{
			FusionLogger.Error($"Failed to load EOS SDK into the application!");
			return;
		}
		else
		{
			FusionLogger.Log($"Successfully loaded EOS SDK into the application!");
			HasEOSSDK = true;
		}
	}

	public static void OnFreeEOSSDK()
	{
		if (!HasEOSSDK)
			return;

		// No Android equivalent, idk 
		if (!PlatformHelper.IsAndroid)
			DllTools.FreeLibrary(_libraryPtr);

		HasEOSSDK = false;
	}

	private static async Task<bool> DownloadLibraryAsync(string libraryName, string output, Action<bool> onComplete = null)
	{
		// change for release
		string url = "https://raw.githubusercontent.com/Checkerb0ard/Fusion-Resources/refs/heads/main/";
		string fileName = string.Empty;

		switch (libraryName)
		{
			case "EOSSDK":
				fileName = "x86_64/EOSSDK-Win64-Shipping.dll";
				break;
			case "libc++_shared":
				fileName = "arm64/libc%2B%2B_shared.so";
				break;
			case "libEOSSDK":
				fileName = "arm64/libEOSSDK.so";
				break;
			default:
				FusionLogger.Error($"Unknown library name: {libraryName}");
				onComplete?.Invoke(false);
				return false;
		}

		string fullUrl = url + fileName;

		FusionLogger.Log($"Downloading {libraryName} from {fullUrl} to {output}");

		HttpClientHandler handler = new HttpClientHandler
		{
			ClientCertificateOptions = ClientCertificateOption.Manual,
			ServerCertificateCustomValidationCallback = (_, _, _, _) => true
		};

		using (var httpClient = new HttpClient(handler))
		{
			try
			{
				var response = await httpClient.GetAsync(fullUrl);
				response.EnsureSuccessStatusCode();

				await using var fileStream = new FileStream(output, FileMode.Create);
				await response.Content.CopyToAsync(fileStream);

				FusionLogger.Log($"Successfully downloaded {libraryName} to {output}");
				onComplete?.Invoke(true);
				return true;
			}
			catch (Exception ex)
			{
				FusionLogger.Error($"Failed to download {libraryName}: {ex.Message}");
				onComplete?.Invoke(false);
				return false;
			}
		}
	}

	private static void InitializeAndroidJNI()
	{
		try
		{
			JClass systemClass = JNI.FindClass("java/lang/System");
			if (systemClass == null)
			{
				FusionLogger.Error("Failed to find java.lang.System class");
				return;
			}

			JMethodID loadMethod = JNI.GetStaticMethodID(systemClass, "load", "(Ljava/lang/String;)V");
			if (loadMethod.Handle == IntPtr.Zero)
			{
				FusionLogger.Error("Failed to find System.load method");
				return;
			}

			string fullLibPath = PersistentData.GetPath("libEOSSDK.so");
			JString libPath = JNI.NewString(fullLibPath);

			FusionLogger.Log($"Calling System.load with path: {fullLibPath}");

			JNI.CallStaticVoidMethod(systemClass, loadMethod, new JValue(libPath));

			FusionLogger.Log("System.load");
		}
		catch (Exception ex)
		{
			FusionLogger.Error($"Failed to initialize JNI: {ex.Message}");
		}
	}
}