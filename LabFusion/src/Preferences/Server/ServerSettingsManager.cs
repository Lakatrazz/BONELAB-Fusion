using LabFusion.Utilities;

using MelonLoader;

namespace LabFusion.Preferences.Server;

public static class ServerSettingsManager
{
    private static ServerSettings _savedSettings = null;
    public static ServerSettings SavedSettings => _savedSettings;

    private static ServerSettings _hostSettings = null;
    public static ServerSettings HostSettings 
    {
        get
        {
            return _hostSettings;
        }
        set
        {
            _hostSettings = value;

            PushSettingsUpdate();
        }
    }

    public static ServerSettings ActiveSettings => HostSettings ?? SavedSettings;

    public static event Action OnServerSettingsChanged;

    public static void OnInitialize(MelonPreferences_Category category)
    {
        // Get the locally saved server settings
        _savedSettings = ServerSettings.CreateMelonPrefs(category);
    }

    public static void PushSettingsUpdate()
    {
        OnServerSettingsChanged.InvokeSafe("executing server settings changed hook");
    }
}