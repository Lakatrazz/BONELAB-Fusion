using LabFusion.Utilities;

namespace LabFusion.Data;

public static class EOSSDKLoader
{
    public static bool HasEOSSDK { get; private set; } = false;
    
    internal static IntPtr LibraryPtr { get; private set; } = IntPtr.Zero;

    public static void OnLoadEOSSDK()
    {
        // If it's already loaded, don't load it again
        if (HasEOSSDK)
        {
            return;
        }
        
        string sdkPath = PersistentData.GetPath(PlatformHelper.IsAndroid ? "libEOSSDK.so" : "EOSSDK-Win64-Shipping.dll");
        
        ExtractAPI(sdkPath, false);
        
        if (TryLoadSDK(sdkPath, out var libraryPtr, out var errorCode))
        {
            OnLoadAPI(libraryPtr);
        }
        else if (errorCode == 193)
        {
            FusionLogger.Error("EOSSDK was corrupted, attempting re-extraction...");

            ExtractAPI(sdkPath, true);

            if (TryLoadSDK(sdkPath, out libraryPtr, out _))
            {
                OnLoadAPI(libraryPtr);
            }
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
    
    private static void ExtractAPI(string path, bool overwrite = false)
    {
        if (!File.Exists(path) || overwrite)
        {
            File.WriteAllBytes(path, EmbeddedResource.LoadBytesFromAssembly(FusionMod.FusionAssembly, PlatformHelper.IsAndroid ? ResourcePaths.EOSSDKAndroidPath : ResourcePaths.EOSSDKWindowsPath));
        }
        else
        {
            FusionLogger.Log("EOSSDK already exists, skipping extraction.");
        }
    }
    
    private static bool TryLoadSDK(string path, out IntPtr libraryPtr, out uint errorCode)
    {
        errorCode = 0;

        libraryPtr = MelonLoader.NativeLibrary.LoadLib(path);

        if (libraryPtr != IntPtr.Zero)
        {
            return true;
        }
        else
        {
            if (PlatformHelper.IsAndroid)
            {
                errorCode = (uint)DllTools.dlerror();
            }
            else
            {
                errorCode = DllTools.GetLastError();
            }
            return false;
        }
    }
    
    private static void OnLoadAPI(IntPtr libraryPtr)
    {
        LibraryPtr = libraryPtr;

        FusionLogger.Log("Successfully loaded EOSSDK into the application!");
        HasEOSSDK = true;
    }
}