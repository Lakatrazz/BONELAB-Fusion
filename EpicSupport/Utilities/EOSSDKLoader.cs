using System.Runtime.InteropServices;
using System.Security.Cryptography;
using LabFusion.Data;
using LabFusion.Utilities;

namespace MarrowFusion.Epic.Utilities;

public static class EOSSDKLoader
{
    public static bool HasEOSSDK { get; private set; } = false;
    
    internal static IntPtr LibraryPtr { get; private set; } = IntPtr.Zero;
    
    private const string EOSSDKWindowsPath = "EpicSupport.dependencies.resources.lib.x86_64.EOSSDK-Win64-Shipping.dll";
    private const string EOSSDKAndroidPath = "EpicSupport.dependencies.resources.lib.arm64.libEOSSDK.so";
    
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
            EpicModule.Logger.Error("EOSSDK was corrupted, attempting re-extraction...");

            ExtractAPI(sdkPath, true);

            if (TryLoadSDK(sdkPath, out libraryPtr, out _))
            {
                OnLoadAPI(libraryPtr);
            }
        }
    }
    
    private static void ExtractAPI(string path, bool overwrite = false)
    {
        byte[] embeddedBytes = EmbeddedResource.LoadBytesFromAssembly(EpicModule.ModuleAssembly, PlatformHelper.IsAndroid ? EOSSDKAndroidPath : EOSSDKWindowsPath);

        if (!File.Exists(path) || overwrite || !FilesMatch(path, embeddedBytes))
        {
            File.WriteAllBytes(path, embeddedBytes);
        }
        else
        {
            EpicModule.Logger.Log("EOSSDK already exists, skipping extraction.");
        }
    }
    
    private static bool FilesMatch(string filePath, byte[] embeddedBytes)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        byte[] diskHash = sha256.ComputeHash(stream);
        byte[] embeddedHash = sha256.ComputeHash(embeddedBytes);

        return diskHash.SequenceEqual(embeddedHash);
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
                errorCode = (uint)dlerror();
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

        EpicModule.Logger.Log("Successfully loaded EOSSDK into the application!");
        HasEOSSDK = true;
    }
    
    [DllImport("libdl.so")]
    private static extern IntPtr dlerror();
}