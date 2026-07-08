using MelonLoader;

namespace LabFusion.Preferences.Client;

public class NotificationSettings
{
    public FusionPref<bool> NotifyServerStarted { get; private set; }

    public FusionPref<bool> NotifyServerJoined { get; private set; }

    public FusionPref<bool> NotifyServerLeft { get; private set; }

    public FusionPref<bool> NotifyPlayerJoined { get; private set; }

    public FusionPref<bool> NotifyPlayerLeft { get; private set; }

    public FusionPref<bool> NotifyDownloads { get; private set; }

    public void CreatePrefs(MelonPreferences_Category category)
    {
        NotifyServerStarted = new FusionPref<bool>(category, "Notify Server Started", true, PrefUpdateMode.IGNORE);

        NotifyServerJoined = new FusionPref<bool>(category, "Notify Server Joined", true, PrefUpdateMode.IGNORE);

        NotifyServerLeft = new FusionPref<bool>(category, "Notify Server Left", true, PrefUpdateMode.IGNORE);

        NotifyPlayerJoined = new FusionPref<bool>(category, "Notify Player Joined", true, PrefUpdateMode.IGNORE);

        NotifyPlayerLeft = new FusionPref<bool>(category, "Notify Player Left", true, PrefUpdateMode.IGNORE);

        NotifyDownloads = new FusionPref<bool>(category, "Notify Downloads", true, PrefUpdateMode.IGNORE);
    }
}