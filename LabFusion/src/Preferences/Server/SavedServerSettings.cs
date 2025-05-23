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
    public static FusionPref<bool> NameTags { get; private set; }
    public static FusionPref<bool> VoiceChat { get; private set; }
    public static FusionPref<bool> PlayerConstraining { get; private set; }
    public static FusionPref<ServerPrivacy> Privacy { get; private set; }
    public static FusionPref<TimeScaleMode> SlowMoMode { get; private set; }
    public static FusionPref<int> MaxPlayers { get; private set; }

    // Visual
    public static FusionPref<string> ServerName { get; private set; }
    public static FusionPref<string> ServerDescription { get; private set; }

    // Combat
    public static FusionPref<bool> Mortality { get; private set; }
    public static FusionPref<bool> FriendlyFire { get; private set; }
    public static FusionPref<bool> Knockout { get; private set; }
    public static FusionPref<int> KnockoutLength { get; private set; }
    public static FusionPref<float> MaxAvatarHeight { get; private set; }

    // Permissions
    public static FusionPref<PermissionLevel> DevTools { get; private set; }
    public static FusionPref<PermissionLevel> Constrainer { get; private set; }
    public static FusionPref<PermissionLevel> CustomAvatars { get; private set; }
    public static FusionPref<PermissionLevel> Kicking { get; private set; }
    public static FusionPref<PermissionLevel> Banning { get; private set; }
    public static FusionPref<PermissionLevel> Teleportation { get; private set; }

    public static event Action OnSavedServerSettingsChanged;

    public static void OnInitialize(MelonPreferences_Category category)
    {
        var updateMode = PrefUpdateMode.SERVER_UPDATE;

        // General settings
        NameTags = new FusionPref<bool>(category, "Server Nametags Enabled", true, updateMode);
        VoiceChat = new FusionPref<bool>(category, "Server Voicechat Enabled", true, updateMode);
        PlayerConstraining = new FusionPref<bool>(category, "Server Player Constraints Enabled", false, updateMode);
        Privacy = new FusionPref<ServerPrivacy>(category, "Server Privacy", ServerPrivacy.PUBLIC, updateMode);
        SlowMoMode = new FusionPref<TimeScaleMode>(category, "Time Scale Mode", TimeScaleMode.LOW_GRAVITY, updateMode);
        MaxPlayers = new FusionPref<int>(category, "Max Players", 10, updateMode);

        // Visual
        ServerName = new FusionPref<string>(category, "Server Name", string.Empty, updateMode);
        ServerDescription = new FusionPref<string>(category, "Server Description", string.Empty, updateMode);

        // Combat
        Mortality = new FusionPref<bool>(category, "Server Mortality", true, updateMode);
        FriendlyFire = new FusionPref<bool>(category, "Friendly Fire", true, updateMode);
        Knockout = new FusionPref<bool>(category, "Knockout", false, updateMode);
        KnockoutLength = new FusionPref<int>(category, "Knockout Length", 10, updateMode);
        MaxAvatarHeight = new FusionPref<float>(category, "Max Avatar Height", 20f, updateMode);

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
