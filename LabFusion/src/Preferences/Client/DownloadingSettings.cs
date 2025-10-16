using MelonLoader;
using UnityEngine;

namespace LabFusion.Preferences.Client;

public class DownloadingSettings
{
    public const int DefaultMaxFileSize = 2000;
    public const int DefaultMaxLevelSize = 4000;

    public FusionPref<bool> DownloadSpawnables { get; private set; }
    public FusionPref<bool> DownloadAvatars { get; private set; }
    public FusionPref<bool> DownloadLevels { get; private set; }

    public FusionPref<bool> KeepDownloadedMods { get; private set; }

    public FusionPref<bool> NotifyDownloads { get; private set; }

    public FusionPref<int> MaxFileSize { get; private set; }
    public FusionPref<int> MaxLevelSize { get; private set; }

    public FusionPref<bool> DownloadMatureContent { get; private set; }

    public FusionPref<string> ModsPath { get; private set; }
    public FusionPref<string> RootDownloadPath { get; private set; }

    public FusionPref<bool> ClearDownloadCache { get; private set; }

    public void CreatePrefs(MelonPreferences_Category category)
    {
        DownloadSpawnables = new FusionPref<bool>(category, "Download Spawnables", true, PrefUpdateMode.IGNORE);
        DownloadAvatars = new FusionPref<bool>(category, "Download Avatars", true, PrefUpdateMode.IGNORE);
        DownloadLevels = new FusionPref<bool>(category, "Download Levels", true, PrefUpdateMode.IGNORE);

        KeepDownloadedMods = new FusionPref<bool>(category, "Keep Downloaded Mods", false, PrefUpdateMode.IGNORE);

        NotifyDownloads = new FusionPref<bool>(category, "Notify Downloads", true, PrefUpdateMode.IGNORE);

        MaxFileSize = new FusionPref<int>(category, "Max File Size", DefaultMaxFileSize, PrefUpdateMode.IGNORE);
        MaxLevelSize = new FusionPref<int>(category, "Max Level Size", DefaultMaxLevelSize, PrefUpdateMode.IGNORE);

        DownloadMatureContent = new FusionPref<bool>(category, "Download Mature Content", false, PrefUpdateMode.IGNORE);

        ModsPath = new FusionPref<string>(category, "Mods Path", Application.persistentDataPath + "/Mods", PrefUpdateMode.IGNORE);
        RootDownloadPath = new FusionPref<string>(category, "Root Download Path", Application.persistentDataPath, PrefUpdateMode.IGNORE);

        ClearDownloadCache = new FusionPref<bool>(category, "Clear Download Cache", false, PrefUpdateMode.IGNORE);
    }
}