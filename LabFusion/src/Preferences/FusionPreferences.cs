using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Client;

using MelonLoader;

namespace LabFusion.Preferences;

public static class FusionPreferences
{
    public static MelonPreferences_Category prefCategory;

    public static event Action OnPrefsLoaded;

    internal static void SendServerSettings()
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        using var writer = FusionWriter.Create();
        var data = ServerSettingsData.Create(SerializedServerSettings.Create());
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.ServerSettings, writer);
        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
    }

    internal static void SendServerSettings(ulong longId)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        using var writer = FusionWriter.Create(ServerSettingsData.Size);
        var data = ServerSettingsData.Create(SerializedServerSettings.Create());
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.ServerSettings, writer);
        MessageSender.SendFromServer(longId, NetworkChannel.Reliable, message);
    }

    internal static void SendClientSettings()
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        using var writer = FusionWriter.Create(PlayerSettingsData.Size);
        var data = PlayerSettingsData.Create(PlayerIdManager.LocalSmallId, SerializedPlayerSettings.Create());
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PlayerSettings, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }

    internal static void OnInitializePreferences()
    {
        // Create preferences
        prefCategory = MelonPreferences.CreateCategory("BONELAB Fusion");

        ServerSettingsManager.OnInitialize(prefCategory);

        ClientSettings.OnInitialize(prefCategory);

        // Save category
        prefCategory.SaveToFile(false);
    }

    internal static void OnPreferencesLoaded()
    {
        OnPrefsLoaded?.Invoke();
    }
}