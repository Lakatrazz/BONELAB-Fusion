using LabFusion.Utilities;
using MelonLoader;

namespace LabFusion.Data;

public static class EOSSDKLoader
{
    public static bool HasEOSSDK { get; private set; } = false;
    private static string libEOSSDKPath;
    private static string libCPlusPlus;
    private static string libPath;

    private static IntPtr _libraryPtr;

    public static void OnLoadEOSSDK()
    {
        // If it's already loaded, don't load it again
        if (HasEOSSDK)
        {
            return;
        }

        string eosSDKPath;

        if (PlatformHelper.IsAndroid)
        {
            eosSDKPath = PersistentData.GetPath($"libEOSSDK.so");
            libEOSSDKPath = "LabFusion.dependencies.resources.lib.arm64.libEOSSDK.so";
            libPath = PersistentData.GetPath($"libc++_shared.so");
            libCPlusPlus = "LabFusion.dependencies.resources.lib.arm64.libc++_shared.so";

            Extract(libPath, false, libCPlusPlus);
        }
        else
        {
            eosSDKPath = PersistentData.GetPath($"EOSSDK.dll");
            libEOSSDKPath = "LabFusion.dependencies.resources.lib.x86_64.EOSSDK-Win64-Shipping.dll";
        }

        Extract(eosSDKPath, false, libEOSSDKPath);

        if (TryLoadEOSSDK(eosSDKPath, out var libraryPtr, out var errorCode))
        {
            OnLoadEOSSDK(libraryPtr);
        }
        // 193 is a corrupted file
        else if (errorCode == 193)
        {
            FusionLogger.Error("EOS SDK was corrupted, attempting re-extraction...");

            Extract(eosSDKPath, true, libEOSSDKPath);

            if (TryLoadEOSSDK(eosSDKPath, out libraryPtr, out _))
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

        //DllTools.FreeLibrary(_libraryPtr);

        HasEOSSDK = false;
    }

    private static void Extract(string path, bool overwrite, string libPath)
    {
        if (!File.Exists(path) || overwrite)
        {
            File.WriteAllBytes(path, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, libPath));
        }
        else
        {
            FusionLogger.Log("EOS SDK already exists, skipping extraction.");
        }
    }

    private static bool TryLoadEOSSDK(string path, out IntPtr libraryPtr, out uint errorCode)
    {
        errorCode = 0;

        if (PlatformHelper.IsAndroid)
        {
            NativeLibrary.LoadLib(libPath);
        }

        libraryPtr = NativeLibrary.LoadLib(path);

        if (libraryPtr != IntPtr.Zero)
        {
            return true;
        }
        else
        {
            errorCode = 1;
            return false;
        }
    }

    private static void OnLoadEOSSDK(IntPtr libraryPtr)
    {
        _libraryPtr = libraryPtr;

        FusionLogger.Log("Successfully loaded EOS SDK into the application!");
        HasEOSSDK = true;
    }
}