using LabFusion.Utilities;

namespace LabFusion.Data;

public static class EOSSDKLoader
{
    public static bool HasEOSSDK { get; private set; } = false;
    
    internal static IntPtr LibraryPtr { get; private set; } = IntPtr.Zero;

    public static async Task OnLoadEOSSDK()
    {
        // If it's already loaded, don't load it again
        if (HasEOSSDK)
        {
            return;
        }
        
        if (PlatformHelper.IsAndroid)
            await LoadLinuxBionicARM64();
        else
            await LoadWin64();

        if (LibraryPtr == IntPtr.Zero)
        {
            FusionLogger.Error($"Failed to load EOSSDK into the application!");
        }
        else
        {
            FusionLogger.Log($"Successfully loaded EOSSDK into the application!");
            HasEOSSDK = true;
        }
      

        async Task LoadWin64()
        {
            string sdkPath = PersistentData.GetPath("EOSSDK-Win64-Shipping.dll");

            if (!File.Exists(sdkPath))
            {
                bool downloadSuccess = await DownloadLibraryAsync(DownloadCatalog.EOSSDKWin64, sdkPath);
                if (!downloadSuccess)
                {
                    FusionLogger.Error("Failed to download EOSSDK-Win64-Shipping.dll");
                    return;
                }
            }
            else
            {
                FusionLogger.Log("EOSSDK-Win64-Shipping.dll already exists, skipping download.");
            }
            
            LibraryPtr = MelonLoader.NativeLibrary.LoadLib(sdkPath);
        }

        async Task LoadLinuxBionicARM64()
        {
            string sdkPath = PersistentData.GetPath("libEOSSDK.so");
            string cppPath = PersistentData.GetPath("libc++_shared.so");

            if (!File.Exists(sdkPath))
            {
                bool eosDownloadSuccess = await DownloadLibraryAsync(DownloadCatalog.EOSSDKLinuxBionicARM64, sdkPath);
                if (!eosDownloadSuccess)
                {
                    FusionLogger.Error("Failed to download libEOSSDK.so");
                    return;
                }
            }

            if (!File.Exists(cppPath))
            {
                bool cppDownloadSuccess = await DownloadLibraryAsync(DownloadCatalog.CppShared, cppPath);
                if (!cppDownloadSuccess)
                {
                    FusionLogger.Error("Failed to download libc++_shared.so");
                    return;
                }
            }

            if (MelonLoader.NativeLibrary.LoadLib(cppPath) == IntPtr.Zero)
                FusionLogger.Error($"Failed to load libc++_shared.so into the application!");
            else
                FusionLogger.Log($"Successfully loaded libc++_shared.so into the application!");

            LibraryPtr = MelonLoader.NativeLibrary.LoadLib(sdkPath);
        }
    }

    public static void OnFreeEOSSDK()
    {
        if (!HasEOSSDK)
            return;
        
        if (PlatformHelper.IsAndroid)
            DllTools.dlclose(LibraryPtr);
        else
            DllTools.FreeLibrary(LibraryPtr);
        
        HasEOSSDK = false;
    }

    private enum DownloadCatalog
    {
        EOSSDKWin64,
        EOSSDKLinuxBionicARM64,
        CppShared,
    }

    private static async Task<bool> DownloadLibraryAsync(DownloadCatalog file, string output, Action<bool> onComplete = null)
    {
        string url = "https://raw.githubusercontent.com/Checkerb0ard/Fusion-Resources/refs/heads/main/";
        string fileName = string.Empty;

        switch (file)
        {
            case DownloadCatalog.EOSSDKWin64:
                fileName = "x86_64/EOSSDK-Win64-Shipping.dll";
                break;
            case DownloadCatalog.EOSSDKLinuxBionicARM64:
                fileName = "arm64/libEOSSDK.so";
                break;
            case DownloadCatalog.CppShared:
                fileName = "arm64/libc%2B%2B_shared.so";
                break;
            default:
                FusionLogger.Error($"Unknown library name: {file.ToString()}");
                onComplete?.Invoke(false);
                return false;
        }
        
        string fullUrl = url + fileName;
        
        FusionLogger.Log($"Downloading {file.ToString()}...");
        
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

                FusionLogger.Log($"Successfully downloaded {file.ToString()}!");
                onComplete?.Invoke(true);
                return true;
            }
            catch (Exception ex)
            {
                FusionLogger.Error($"Failed to download {file.ToString()}: {ex.Message}");
                onComplete?.Invoke(false);
                return false;
            }
        }
    }
}