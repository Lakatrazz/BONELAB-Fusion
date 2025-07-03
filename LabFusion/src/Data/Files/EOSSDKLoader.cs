using Epic.OnlineServices;

using LabFusion.Utilities;

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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int JNI_OnLoadDelegate(IntPtr javaVM, IntPtr reserved);

    public static unsafe void OnLoadEOSSDK()
    {
        if (HasEOSSDK)
        {
            return;
        }

        string eosSDKPath;
        string libEOSSDKPath;

        if (PlatformHelper.IsAndroid)
        {
            eosSDKPath = PersistentData.GetPath($"libEOSSDK.so");
            libEOSSDKPath = "LabFusion.dependencies.resources.lib.arm64.libEOSSDK.so";

            string libCPPPath = PersistentData.GetPath($"libc++_shared.so");
            Extract(libCPPPath, false, libCPlusPlus);

            if (MelonLoader.NativeLibrary.LoadLib(libCPPPath) == IntPtr.Zero)
                FusionLogger.Error($"Failed to load libc++_shared.so into the application!");
            else
                FusionLogger.Log($"Successfully loaded libc++_shared.so into the application!");
        }
        else
        {
            eosSDKPath = PersistentData.GetPath($"EOSSDK.dll");
            libEOSSDKPath = "LabFusion.dependencies.resources.lib.x86_64.EOSSDK-Win64-Shipping.dll";
        }

        Extract(eosSDKPath, false, libEOSSDKPath);

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
            {
                InitializeAndroidJNI();
            }

            // Set custom Import Resolver since Android is evil and doesn't like DLLImport
            if (PlatformHelper.IsAndroid)
                System.Runtime.InteropServices.NativeLibrary.SetDllImportResolver(typeof(EOSSDKLoader).Assembly, AndroidImportResolver);
        }
    }

    private static void InitializeAndroidJNI()
    {
        try
        {
            Type jniType = Type.GetType("JNISharp.NativeInterface.JNI, JNISharp");
            if (jniType != null)
            {
                var vmPtrField = jniType.GetField("lastVmPtr", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (vmPtrField != null)
                {
                    IntPtr vmPtr = (IntPtr)vmPtrField.GetValue(null);

                    IntPtr onLoadPtr = MelonLoader.NativeLibrary.GetExport(_libraryPtr, "JNI_OnLoad");

                    var onLoad = Marshal.GetDelegateForFunctionPointer<JNI_OnLoadDelegate>(onLoadPtr);
                    int result = onLoad(vmPtr, IntPtr.Zero);
                }
            }
        }
        catch (Exception ex)
        {
            FusionLogger.Error($"Failed to initialize JNI: {ex.Message}");
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

    private static void Extract(string path, bool overwrite, string libPath)
    {
        if (!File.Exists(path) || overwrite)
        {
            File.WriteAllBytes(path, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, libPath));
        }
        else
        {
            FusionLogger.Log("EOSSDK already exists, skipping extraction.");
        }
    }
}