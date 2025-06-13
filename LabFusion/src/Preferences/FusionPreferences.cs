using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Client;
using LabFusion.Preferences.Server;

using MelonLoader;

namespace LabFusion.Preferences;

public static class FusionPreferences
{
    public static MelonPreferences_Category prefCategory;

    public static event Action OnPrefsLoaded;

    internal static void SendClientSettings()
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = PlayerSettingsData.Create(PlayerIDManager.LocalSmallID, SerializedPlayerSettings.Create());

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerSettings, CommonMessageRoutes.ReliableToOtherClients);
    }

    internal static void OnInitializePreferences()
    {
        // Create preferences
        prefCategory = MelonPreferences.CreateCategory("BONELAB Fusion");

        SavedServerSettings.OnInitialize(prefCategory);

        ClientSettings.OnInitialize(prefCategory);

        // Save category
        prefCategory.SaveToFile(false);
    }

    internal static void OnPreferencesLoaded()
    {
        OnPrefsLoaded?.Invoke();
    }
}