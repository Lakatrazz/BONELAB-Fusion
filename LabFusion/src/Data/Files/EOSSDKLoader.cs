using LabFusion.Utilities;

namespace LabFusion.Data;

public static class EOSSDKLoader
{
    public static bool HasEOSSDK { get; private set; } = false;

    private static IntPtr _libraryPtr;

    public static void OnLoadEOSSDK()
    {
        // If it's already loaded, don't load it again
        if (HasEOSSDK)
        {
            return;
        }

        // Don't extract this for android
        if (PlatformHelper.IsAndroid)
        {
            HasEOSSDK = false;
            return;
        }

        // Extracts steam api 64 and loads it into the game
        string sdkPath = PersistentData.GetPath($"EOSSDK-Win64-Shipping.dll");

        Extract(sdkPath, false);

        if (TryLoadEOSSDK(sdkPath, out var libraryPtr, out var errorCode))
        {
            OnLoadEOSSDK(libraryPtr);
        }
        // 193 is a corrupted file
        else if (errorCode == 193)
        {
            FusionLogger.Error("EOSSDK-Win64-Shipping.dll was corrupted, attempting re-extraction...");

            Extract(sdkPath, true);

            if (TryLoadEOSSDK(sdkPath, out libraryPtr, out _))
            {
                OnLoadEOSSDK(libraryPtr);
            }
        }
    }

    public static void OnFreeEOSSDK()
    {
        // Don't unload it if it isn't loaded
        if (!HasEOSSDK)
            return;

        DllTools.FreeLibrary(_libraryPtr);

        HasEOSSDK = false;
    }

    private static void Extract(string path, bool overwrite = false)
    {
        if (!File.Exists(path) || overwrite)
        {
            File.WriteAllBytes(path, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.EOSSDKPath));
        }
        else
        {
            FusionLogger.Log("EOSSDK-Win64-Shipping.dll already exists, skipping extraction.");
        }
    }

    private static bool TryLoadEOSSDK(string path, out IntPtr libraryPtr, out uint errorCode)
    {
        errorCode = 0;

        libraryPtr = DllTools.LoadLibrary(path);

        if (libraryPtr != IntPtr.Zero)
        {
            return true;
        }
        else
        {
            errorCode = DllTools.GetLastError();
            return false;
        }
    }

    private static void OnLoadEOSSDK(IntPtr libraryPtr)
    {
        _libraryPtr = libraryPtr;

        FusionLogger.Log("Successfully loaded EOSSDK-Win64-Shipping.dll into the application!");
        HasEOSSDK = true;
    }
}