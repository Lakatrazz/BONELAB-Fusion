using System.Runtime.InteropServices;

using LabFusion.Data;
using LabFusion.Utilities;

namespace MarrowFusion.Epic.Utilities;

public static class EOSSDKLoader
{
    public static bool IsSDKLoaded { get; private set; } = false;
    
    internal static IntPtr LibraryPtr { get; private set; } = IntPtr.Zero;
    
    public static string GetSDKRelativePath() => PlatformHelper.IsAndroid ? EpicResourcePaths.EOSSDKAndroidRelativePath : EpicResourcePaths.EOSSDKWindowsRelativePath;

    public static string GetSDKAbsolutePath() => ExternalResources.GetAbsolutePath(GetSDKRelativePath());

    public static bool TryGetSDKAbsolutePath(out string absolutePath)
    {
        absolutePath = null;

        if (!ExternalResources.IsAvailable)
        {
            return false;
        }

        absolutePath = GetSDKAbsolutePath();
        return File.Exists(absolutePath);
    }

    public static bool IsSDKAvailable()
    {
        if (IsSDKLoaded)
        {
            return true;
        }

        return TryGetSDKAbsolutePath(out _);
    }

    public static bool TryLoadEOSSDK()
    {
        LoadEOSSDK();
        return IsSDKLoaded;
    }

    public static void LoadEOSSDK()
    {
        if (IsSDKLoaded)
        {
            return;
        }

        if (!TryGetSDKAbsolutePath(out var path))
        {
            return;
        }

        if (TryLoadLibrary(path, out var libraryPtr, out var errorCode))
        {
            OnLoadAPI(libraryPtr);
        }
        else
        {
            EpicModule.Logger.Error($"Failed loading EOSSDK with error code {errorCode}.");
        }
    }

    private static bool TryLoadLibrary(string path, out IntPtr libraryPtr, out uint errorCode)
    {
        errorCode = 0;

        libraryPtr = MelonLoader.NativeLibrary.LoadLib(path);

        if (libraryPtr != IntPtr.Zero)
        {
            return true;
        }

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
    
    private static void OnLoadAPI(IntPtr libraryPtr)
    {
        LibraryPtr = libraryPtr;

        EpicModule.Logger.Log("Successfully loaded EOSSDK into the application!");
        IsSDKLoaded = true;
    }
    
    [DllImport("libdl.so")]
    private static extern IntPtr dlerror();
}