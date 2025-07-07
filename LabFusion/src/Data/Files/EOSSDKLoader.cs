using JNISharp.NativeInterface;
using LabFusion.Utilities;
using System.Runtime.InteropServices;

namespace LabFusion.Data;

public static class EOSSDKLoader
{
	public static bool HasEOSSDK { get; private set; } = false;

	internal static IntPtr JavaVM { get; private set; } = IntPtr.Zero;

    private static IntPtr _libraryPtr = IntPtr.Zero;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int JNI_OnLoadDelegate(IntPtr javaVM, IntPtr reserved);

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

		_libraryPtr = MelonLoader.NativeLibrary.LoadLib(eosSDKPath);

		if (_libraryPtr == IntPtr.Zero)
		{
			FusionLogger.Error($"Failed to load EOS SDK into the application!");
			return;
		}
		else
		{
			FusionLogger.Log($"Successfully loaded EOS SDK into the application!");

            if (PlatformHelper.IsAndroid)
            {
                JavaVM = (IntPtr)typeof(JNISharp.NativeInterface.JNI).GetField("lastVmPtr", HarmonyLib.AccessTools.all).GetValue(null);
                if (JavaVM == IntPtr.Zero)
                {
                    FusionLogger.Error("Failed to get Java VM pointer from JNISharp!");
                    return;
                }

                IntPtr onLoadPtr = MelonLoader.NativeLibrary.GetExport(_libraryPtr, "JNI_OnLoad");

                var onLoad = Marshal.GetDelegateForFunctionPointer<JNI_OnLoadDelegate>(onLoadPtr);
                int result = onLoad(JavaVM, IntPtr.Zero);

#if DEBUG
                FusionLogger.Log($"JNI_OnLoad returned: {result}");
#endif
                JClass systemClass = JNI.FindClass("java/lang/System");
                JMethodID loadMethod = JNI.GetStaticMethodID(systemClass, "load", "(Ljava/lang/String;)V");
				var libPath = JNI.NewString("/data/data/com.StressLevelZero.BONELAB/libEOSSDK.so");
                JNI.CallStaticVoidMethod(systemClass, loadMethod, libPath);
                if (JNI.ExceptionCheck())
                {
					JNI.ExceptionDescribe();
                    FusionLogger.Error("Failed to call loadLibrary for libEOSSDK!");
                }
                else
                    FusionLogger.Log("EOS SDK initialized successfully in Java.");
            }

            HasEOSSDK = true;
		}
	}

	public static void OnFreeEOSSDK()
	{
		if (!HasEOSSDK)
			return;

		if (!PlatformHelper.IsAndroid)
			DllTools.FreeLibrary(_libraryPtr);
		else 
			DllTools.dlclose(_libraryPtr);

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
}