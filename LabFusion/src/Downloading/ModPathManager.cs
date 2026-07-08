using LabFusion.Preferences.Client;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Downloading;

public static class ModPathManager
{
    /// <summary>
    /// The regular mods folder. This contains mods installed for the base game and persistent mods.
    /// </summary>
    public static string ModsPath => Application.persistentDataPath + "/Mods";

    /// <summary>
    /// The root folder that contains the net mods folder.
    /// </summary>
    public static string RootPath { get; private set; } = Application.persistentDataPath;

    /// <summary>
    /// The net mods folder. This contains all of the other folders pertaining to multiplayer downloaded mods.
    /// </summary>
    public static string NetModsPath => RootPath + "/NetMods";

    /// <summary>
    /// The staging folder. This is where mods are held for downloading and extracting.
    /// </summary>
    public static string StagingPath => NetModsPath + "/Staging";

    /// <summary>
    /// The download folder. This is where the raw downloaded mod zip is downloaded into.
    /// </summary>
    public static string DownloadPath => StagingPath + "/Downloads";

    /// <summary>
    /// The export folder. This is where downloaded mods are extracted into before being moved to the cache folder.
    /// </summary>
    public static string ExportPath => StagingPath + "/Exports";

    /// <summary>
    /// The cache folder. This is where downloaded mods are stored and loaded from.
    /// </summary>
    public static string CachePath => NetModsPath + "/Cache";

    /// <summary>
    /// The file extension for the json file of a pallet.
    /// </summary>
    public static readonly string PalletExtension = ".pallet.json";

    /// <summary>
    /// Updates the root path based on user and game settings.
    /// </summary>
    public static void UpdateRootPath() => RootPath = DetermineRootPath();

    /// <summary>
    /// Creates missing directories for mod installation, if needed.
    /// </summary>
    public static void CreateDirectories()
    {
        if (!Directory.Exists(NetModsPath))
        {
            Directory.CreateDirectory(NetModsPath);
        }

        if (!Directory.Exists(DownloadPath))
        {
            Directory.CreateDirectory(DownloadPath);
        }

        if (!Directory.Exists(ExportPath))
        {
            Directory.CreateDirectory(ExportPath);
        }

        if (!Directory.Exists(CachePath))
        {
            Directory.CreateDirectory(CachePath);
        }
    }

    /// <summary>
    /// Returns the amount of free space on the drive containing downloaded mods, in bytes.
    /// </summary>
    /// <returns></returns>
    public static long GetAvailableFreeSpace()
    {
        var driveName = Path.GetPathRoot(RootPath);

        if (string.IsNullOrWhiteSpace(driveName))
        {
            return 0;
        }

        var drive = new DriveInfo(driveName);

        if (!drive.IsReady)
        {
            return 0;
        }

        return drive.AvailableFreeSpace;
    }

    /// <summary>
    /// Returns if there is enough space for a file to be downloaded given a size in bytes.
    /// </summary>
    /// <param name="fileSize"></param>
    /// <returns></returns>
    public static bool HasEnoughSpace(long fileSize)
    {
        var freeSpace = GetAvailableFreeSpace();
        var allocatedSize = fileSize * 1.2;

        return freeSpace >= allocatedSize;
    }

    /// <summary>
    /// Finds the path to a pallet json given a parent directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    public static string FindPalletJson(string directory)
    {
        foreach (var file in Directory.GetFiles(directory))
        {
            if (file.EndsWith(ModPathManager.PalletExtension))
            {
                return file;
            }
        }

        foreach (var subDirectory in Directory.GetDirectories(directory))
        {
            var file = FindPalletJson(subDirectory);

            if (!string.IsNullOrEmpty(file))
            {
                return file;
            }
        }

        return string.Empty;
    }

    internal static void Initialize()
    {
        DetermineRootPath();

        ClientSettings.Downloading.DownloadPathOverride.OnValueChanged += OnDownloadPathOverrideChanged;
    }

    private static void OnDownloadPathOverrideChanged(string value) => DetermineRootPath();

    private static string DetermineRootPath()
    {
        string defaultPath = Application.persistentDataPath;

        if (PlatformHelper.IsAndroid)
        {
            return defaultPath;
        }

        string overridePath = ClientSettings.Downloading.DownloadPathOverride.Value;

        if (string.IsNullOrWhiteSpace(overridePath))
        {
            return defaultPath;
        }

        if (!Directory.Exists(overridePath))
        {
            return defaultPath;
        }

        return overridePath;
    }
}
