using LabFusion.Data;
using LabFusion.Utilities;

namespace MarrowFusion.Steam;

public static class SteamAPILoader
{
    public static bool IsAPILoaded { get; private set; } = false;

    public static IntPtr LibraryPtr { get; private set; } = IntPtr.Zero;

    public static string GetAPIRelativePath() => SteamResourcePaths.SteamAPIWindowsRelativePath;

    public static string GetAPIAbsolutePath() => ExternalResources.GetAbsolutePath(GetAPIRelativePath());

    public static bool TryGetAPIAbsolutePath(out string absolutePath)
    {
        absolutePath = null;

        if (!ExternalResources.IsAvailable)
        {
            return false;
        }

        if (PlatformHelper.IsAndroid)
        {
            return false;
        }

        absolutePath = GetAPIAbsolutePath();
        return File.Exists(absolutePath);
    }

    public static bool IsAPIAvailable()
    {
        if (IsAPILoaded)
        {
            return true;
        }

        return TryGetAPIAbsolutePath(out _);
    }

    public static bool TryLoadSteamAPI()
    {
        LoadSteamAPI();
        return IsAPILoaded;
    }

    public static void LoadSteamAPI()
    {
        if (IsAPILoaded)
        {
            return;
        }

        if (!TryGetAPIAbsolutePath(out var path))
        {
            return;
        }

        if (TryLoadLibrary(path, out var libraryPtr, out var errorCode))
        {
            OnLoadAPI(libraryPtr);
        }
        else
        {
            SteamModule.Logger.Error($"Failed loading steam api with error code {errorCode}.");
        }
    }

    public static void UnloadSteamAPI()
    {
        if (!IsAPILoaded)
        {
            return;
        }

        DllTools.FreeLibrary(LibraryPtr);

        IsAPILoaded = false;
    }

    private static bool TryLoadLibrary(string path, out IntPtr libraryPtr, out uint errorCode)
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

    private static void OnLoadAPI(IntPtr libraryPtr)
    {
        LibraryPtr = libraryPtr;

        SteamModule.Logger.Log("Successfully loaded steam_api64.dll into the application!");
        IsAPILoaded = true;
    }
}
