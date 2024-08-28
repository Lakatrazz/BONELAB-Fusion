using MelonLoader;

namespace LabFusion.Preferences.Client;

public class VoiceChatSettings
{
    public FusionPref<bool> Muted { get; private set; }
    public FusionPref<bool> MutedIndicator { get; private set; }
    public FusionPref<bool> Deafened { get; private set; }
    public FusionPref<float> GlobalVolume { get; private set; }

    public FusionPref<string> InputDevice { get; private set; }

    public void CreatePrefs(MelonPreferences_Category category)
    {
        Muted = new FusionPref<bool>(category, "Muted", false, PrefUpdateMode.IGNORE);
        MutedIndicator = new FusionPref<bool>(category, "Muted Indicator", true, PrefUpdateMode.IGNORE);
        Deafened = new FusionPref<bool>(category, "Deafened", false, PrefUpdateMode.IGNORE);
        GlobalVolume = new FusionPref<float>(category, "GlobalMicVolume", 1f, PrefUpdateMode.IGNORE);

        InputDevice = new FusionPref<string>(category, "Input Device", string.Empty, PrefUpdateMode.IGNORE);
    }
}