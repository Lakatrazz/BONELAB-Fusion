using MelonLoader;

namespace LabFusion.Preferences.Client;

public class SafetySettings
{
    public FusionPref<bool> FilterProfanity { get; private set; }

    public void CreatePrefs(MelonPreferences_Category category)
    {
        FilterProfanity = new FusionPref<bool>(category, "Filter Profanity", true, PrefUpdateMode.IGNORE);
    }
}