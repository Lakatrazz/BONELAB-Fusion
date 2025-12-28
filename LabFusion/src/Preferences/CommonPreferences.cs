using LabFusion.Senders;
using LabFusion.Preferences.Client;
using LabFusion.Network;

namespace LabFusion.Preferences;

public static class CommonPreferences
{
    public static bool NameTags => LobbyInfoManager.LobbyInfo.NameTags && ClientSettings.NameTags.Value;
    public static bool Mortality => LobbyInfoManager.LobbyInfo.Mortality;
    public static bool Knockout => LobbyInfoManager.LobbyInfo.Knockout;
    public static TimeScaleMode SlowMoMode => LobbyInfoManager.LobbyInfo.SlowMoMode;

    public static readonly bool ShowMatureMods = false;
}