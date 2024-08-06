using LabFusion.Utilities;
using MelonLoader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Preferences;

public static class ServerSettingsManager
{
    private static ServerSettings _savedSettings = null;
    public static ServerSettings SavedSettings => _savedSettings;

    private static ServerSettings _hostSettings = null;
    public static ServerSettings HostSettings => _hostSettings;

    public static ServerSettings ActiveSettings => HostSettings ?? SavedSettings;

    public static void OnInitialize(MelonPreferences_Category category)
    {
        // Get the locally saved server settings
        _savedSettings = ServerSettings.CreateMelonPrefs(category);
    }

    public static void OnReceiveHostSettings(ServerSettings settings)
    {
        _hostSettings = settings;

        MultiplayerHooking.Internal_OnServerSettingsChanged();
    }
}