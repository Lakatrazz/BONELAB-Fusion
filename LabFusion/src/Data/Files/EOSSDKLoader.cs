using Epic.OnlineServices;

using LabFusion.Utilities;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LabFusion.Data;

public static class EOSSDKLoader
{
	public static bool HasEOSSDK { get; private set; } = false;

	private static IntPtr _libraryPtr = IntPtr.Zero;
	public static IntPtr vm;

	private static IntPtr AndroidImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (libraryName == Config.LibraryName + ".so")
			return _libraryPtr;

		return IntPtr.Zero;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate nint JNI_OnLoadFunc(IntPtr javaVM, IntPtr reserved);
    public static T? GetExport<T>(nint hModule, string name) where T : Delegate
    {
        return !NativeLibrary.TryGetExport(hModule, name, out var export) ? null : Marshal.GetDelegateForFunctionPointer<T>(export);
    }
    public static bool GetExport<T>(nint hModule, string name, [NotNullWhen(true)] out T? func) where T : Delegate
    {
        func = GetExport<T>(hModule, name);
        return func != null;
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

			bool eosDownloadSuccess = await DownloadLibraryAsync("libEOSSDK", eosSDKPath);
			if (!eosDownloadSuccess)
			{
				FusionLogger.Error("Failed to download libEOSSDK.so");
				return;
			}

			bool cppDownloadSuccess = await DownloadLibraryAsync("libc++_shared", libCPPPath);
			if (!cppDownloadSuccess)
			{
				FusionLogger.Error("Failed to download libc++_shared.so");
				return;
			}

			// Now load the libraries
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
			HasEOSSDK = true;

			if (PlatformHelper.IsAndroid)
				InitializeAndroidJNI();

			// Set custom Import Resolver since Android is evil and doesn't like DLLImport
			if (PlatformHelper.IsAndroid)
				System.Runtime.InteropServices.NativeLibrary.SetDllImportResolver(typeof(EOSSDKLoader).Assembly, AndroidImportResolver);
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
			Type jniType = Type.GetType("JNISharp.NativeInterface.JNI, JNISharp", true);
			if (jniType != null)
			{
				var vmPtrField = jniType.GetField("lastVmPtr", HarmonyLib.AccessTools.all);
				if (vmPtrField != null)
				{
					IntPtr vmPtr = (IntPtr)vmPtrField.GetValue(null);
					vm = vmPtr; // Store the VM pointer for later use
                    FusionLogger.Log($"JNI VM Pointer: {vmPtr}");

                    IntPtr onLoadPtr = MelonLoader.NativeLibrary.GetExport(_libraryPtr, "JNI_OnLoad");
					FusionLogger.Log($"JNI_OnLoad pointer: {onLoadPtr}");

                    if (!GetExport<JNI_OnLoadFunc>(_libraryPtr, "JNI_OnLoad", out var jniOnLoad))
                    {
                        FusionLogger.Log("Can't load Export via JNI_OnLoad");
                        return;
                    }
                    nint a = jniOnLoad(vmPtr, IntPtr.Zero);
					FusionLogger.Log($"JNI_OnLoad returned: {a}");
                }
			}
		}
		catch (Exception ex)
		{
			FusionLogger.Error($"Failed to initialize JNI: {ex.Message}");
		}
	}
}