using MelonLoader;

namespace LabFusion.Preferences.Client;

public class DownloadingSettings
{
    public FusionPref<bool> DownloadSpawnables { get; private set; }
    public FusionPref<bool> DownloadAvatars { get; private set; }
    public FusionPref<bool> DownloadLevels { get; private set; }

    public FusionPref<bool> KeepDownloadedMods { get; private set; }

    public void CreatePrefs(MelonPreferences_Category category)
    {
        DownloadSpawnables = new FusionPref<bool>(category, "Download Spawnables", true, PrefUpdateMode.IGNORE);
        DownloadAvatars = new FusionPref<bool>(category, "Download Avatars", true, PrefUpdateMode.IGNORE);
        DownloadLevels = new FusionPref<bool>(category, "Download Levels", true, PrefUpdateMode.IGNORE);

        KeepDownloadedMods = new FusionPref<bool>(category, "Keep Downloaded Mods", false, PrefUpdateMode.IGNORE);
    }
}