using LabFusion.Utilities;

namespace LabFusion.Data;

public static class SteamAPILoader
{
    public static bool HasSteamAPI { get; private set; } = false;

    private static IntPtr _libraryPtr;

    public static void OnLoadSteamAPI()
    {
        // If it's already loaded, don't load it again
        if (HasSteamAPI)
        {
            return;
        }

        // Don't extract this for android
        if (PlatformHelper.IsAndroid)
        {
            HasSteamAPI = false;
            return;
        }

        // Extracts steam api 64 and loads it into the game
        string apiPath = PersistentData.GetPath($"steam_api64.dll");

        ExtractAPI(apiPath, false);

        if (TryLoadAPI(apiPath, out var libraryPtr, out var errorCode))
        {
            OnLoadAPI(libraryPtr);
        }
        // 193 is a corrupted file
        else if (errorCode == 193)
        {
            FusionLogger.Error("steam_api64.dll was corrupted, attempting re-extraction...");

            ExtractAPI(apiPath, true);

            if (TryLoadAPI(apiPath, out libraryPtr, out _))
            {
                OnLoadAPI(libraryPtr);
            }
        }
    }

    public static void OnFreeSteamAPI()
    {
        // Don't unload it if it isn't loaded
        if (!HasSteamAPI)
            return;

        DllTools.FreeLibrary(_libraryPtr);

        HasSteamAPI = false;
    }

    private static void ExtractAPI(string path, bool overwrite = false)
    {
        if (!File.Exists(path) || overwrite)
        {
            File.WriteAllBytes(path, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.SteamAPIPath));
        }
        else
        {
            FusionLogger.Log("steam_api64.dll already exists, skipping extraction.");
        }
    }

    private static bool TryLoadAPI(string path, out IntPtr libraryPtr, out uint errorCode)
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
        _libraryPtr = libraryPtr;

        FusionLogger.Log("Successfully loaded steam_api64.dll into the application!");
        HasSteamAPI = true;
    }
}