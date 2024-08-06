using LabFusion.Senders;
using LabFusion.Preferences.Client;

namespace LabFusion.Preferences;

public static class CommonPreferences
{
    public static bool NametagsEnabled => ServerSettingsManager.ActiveSettings.NametagsEnabled.Value && ClientSettings.NametagsEnabled.Value;
    public static bool IsMortal => ServerSettingsManager.ActiveSettings.ServerMortality.Value;
    public static TimeScaleMode TimeScaleMode => ServerSettingsManager.ActiveSettings.TimeScaleMode.Value;
}