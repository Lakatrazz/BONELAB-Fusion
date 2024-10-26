using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using MelonLoader;

namespace LabFusion.Preferences.Server;

/// <summary>
/// The local user's server settings. If you are looking for the active server settings, see <see cref="LobbyInfoManager.LobbyInfo"/>.
/// </summary>
public static class SavedServerSettings
{
    // General settings
    public static FusionPref<bool> NameTags { get; internal set; }
    public static FusionPref<bool> VoiceChat { get; internal set; }
    public static FusionPref<bool> PlayerConstraints { get; internal set; }
    public static FusionPref<ServerPrivacy> Privacy { get; internal set; }
    public static FusionPref<TimeScaleMode> SlowMoMode { get; internal set; }
    public static FusionPref<int> MaxPlayers { get; internal set; }

    // Visual
    public static FusionPref<string> ServerName { get; internal set; }
    public static FusionPref<string> ServerDescription { get; internal set; }

    // Mortality
    public static FusionPref<bool> Mortality { get; internal set; }

    // Permissions
    public static FusionPref<PermissionLevel> DevTools { get; internal set; }
    public static FusionPref<PermissionLevel> Constrainer { get; internal set; }
    public static FusionPref<PermissionLevel> CustomAvatars { get; internal set; }
    public static FusionPref<PermissionLevel> Kicking { get; internal set; }
    public static FusionPref<PermissionLevel> Banning { get; internal set; }
    public static FusionPref<PermissionLevel> Teleportation { get; internal set; }

    public static event Action OnSavedServerSettingsChanged;

    public static void OnInitialize(MelonPreferences_Category category)
    {
        var updateMode = PrefUpdateMode.SERVER_UPDATE;

        // General settings
        NameTags = new FusionPref<bool>(category, "Server Nametags Enabled", true, updateMode);
        VoiceChat = new FusionPref<bool>(category, "Server Voicechat Enabled", true, updateMode);
        PlayerConstraints = new FusionPref<bool>(category, "Server Player Constraints Enabled", false, updateMode);
        Privacy = new FusionPref<ServerPrivacy>(category, "Server Privacy", ServerPrivacy.PUBLIC, updateMode);
        SlowMoMode = new FusionPref<TimeScaleMode>(category, "Time Scale Mode", Senders.TimeScaleMode.LOW_GRAVITY, updateMode);
        MaxPlayers = new FusionPref<int>(category, "Max Players", 10, updateMode);

        // Visual
        ServerName = new FusionPref<string>(category, "Server Name", string.Empty, updateMode);
        ServerDescription = new FusionPref<string>(category, "Server Description", string.Empty, updateMode);

        // Mortality
        Mortality = new FusionPref<bool>(category, "Server Mortality", true, updateMode);

        // Server permissions
        DevTools = new FusionPref<PermissionLevel>(category, "Dev Tools Allowed", PermissionLevel.DEFAULT, updateMode);
        Constrainer = new FusionPref<PermissionLevel>(category, "Constrainer Allowed", PermissionLevel.DEFAULT, updateMode);
        CustomAvatars = new FusionPref<PermissionLevel>(category, "Custom Avatars Allowed", PermissionLevel.DEFAULT, updateMode);
        Kicking = new FusionPref<PermissionLevel>(category, "Kicking Allowed", PermissionLevel.OPERATOR, updateMode);
        Banning = new FusionPref<PermissionLevel>(category, "Banning Allowed", PermissionLevel.OPERATOR, updateMode);

        Teleportation = new FusionPref<PermissionLevel>(category, "Teleportation", PermissionLevel.OPERATOR, updateMode);
    }

    public static void PushSettingsUpdate()
    {
        OnSavedServerSettingsChanged.InvokeSafe("executing saved server settings changed hook");
    }
}
