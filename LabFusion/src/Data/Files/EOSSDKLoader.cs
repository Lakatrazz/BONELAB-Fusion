using Epic.OnlineServices;
using JNISharp.NativeInterface;
using LabFusion.Utilities;
using MelonLoader;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LabFusion.Data;

public static class EOSSDKLoader
{
    public static bool HasEOSSDK { get; private set; } = false;
    private const string libCPlusPlus = "LabFusion.dependencies.resources.lib.arm64.libc++_shared.so";

    private static IntPtr _libraryPtr = IntPtr.Zero;


    private static IntPtr AndroidImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == Config.LibraryName + ".so")
            return _libraryPtr;
            

        return IntPtr.Zero;
    }

    public static void OnLoadEOSSDK()
    {
        // If it's already loaded, don't load it again
        if (HasEOSSDK)
        {
            return;
        }

        string eosSDKPath;
        string libEOSSDKPath;

        if (PlatformHelper.IsAndroid)
        {
            // Get EOS SDK Path
            eosSDKPath = PersistentData.GetPath($"EOSSDK.so");
            libEOSSDKPath = "LabFusion.dependencies.resources.lib.arm64.libEOSSDK.so";

            // Extract libc++_shared for android
            string libCPPPath = PersistentData.GetPath($"libc++_shared.so");
            Extract(libCPPPath, false, libCPlusPlus);

            if (MelonLoader.NativeLibrary.LoadLib(libCPPPath) == IntPtr.Zero)
                FusionLogger.Error($"Failed to load libc++_shared.so from {libCPPPath}"); 
            else
                FusionLogger.Log($"Successfully loaded libc++_shared.so from {libCPPPath}");
        }
        else
        {
            // Get EOS SDK Path
            eosSDKPath = PersistentData.GetPath($"EOSSDK.dll");
            libEOSSDKPath = "LabFusion.dependencies.resources.lib.x86_64.EOSSDK-Win64-Shipping.dll";
        }

        // Extract EOS SDK to persistent data
        Extract(eosSDKPath, false, libEOSSDKPath);

        _libraryPtr = MelonLoader.NativeLibrary.LoadLib(eosSDKPath);

        if (_libraryPtr == IntPtr.Zero)
        {
            FusionLogger.Error($"Failed to load EOS SDK from {eosSDKPath}");
            return;
        }
        else
        {
            FusionLogger.Log($"Successfully loaded EOS SDK from {eosSDKPath}");
            HasEOSSDK = true;

            if (PlatformHelper.IsAndroid)
            {
                JClass systemClass = JNI.FindClass("java/lang/System");
                JMethodID loadLibraryId = JNI.GetStaticMethodID(systemClass, "loadLibrary", "(Ljava/lang/String;)V");
                JValue[] parameters = new JValue[] { new(eosSDKPath) };

                JNI.CallStaticVoidMethod(systemClass, loadLibraryId, parameters);
            }
        }

        // Set custom Import Resolver since Android doesn't like the DLLImport
        if (PlatformHelper.IsAndroid)
            System.Runtime.InteropServices.NativeLibrary.SetDllImportResolver(typeof(EOSSDKLoader).Assembly, AndroidImportResolver);
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
}