using MelonLoader;

namespace LabFusion.Preferences.Client;

public class WearableSettings
{
    public FusionPref<bool> ShowWristWatch { get; private set; }

    public FusionPref<bool> ShowWristWatchInGamemodes { get; private set; }

    public void CreatePrefs(MelonPreferences_Category category)
    {
        ShowWristWatch = new FusionPref<bool>(category, "Show Wrist Watch", true, PrefUpdateMode.IGNORE);

        ShowWristWatchInGamemodes = new FusionPref<bool>(category, "Show Wrist Watch in Gamemodes", true, PrefUpdateMode.IGNORE);
    }
}