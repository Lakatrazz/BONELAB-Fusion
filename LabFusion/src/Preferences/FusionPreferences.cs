using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;

using MelonLoader;

using UnityEngine;

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

        // Client settings
        ClientSettings.NetworkLayerTitle = new FusionPref<string>(prefCategory, "Network Layer Title", NetworkLayerDeterminer.GetDefaultLayer().Title, PrefUpdateMode.IGNORE);
        ClientSettings.ProxyPort = new FusionPref<int>(prefCategory, "Proxy Port", 28340, PrefUpdateMode.IGNORE);

        // Nametag
        ClientSettings.NametagsEnabled = new FusionPref<bool>(prefCategory, "Client Nametags Enabled", true, PrefUpdateMode.LOCAL_UPDATE);
        ClientSettings.NametagColor = new FusionPref<Color>(prefCategory, "Nametag Color", Color.white, PrefUpdateMode.CLIENT_UPDATE);

        // Nickname
        ClientSettings.Nickname = new FusionPref<string>(prefCategory, "Nickname", "", PrefUpdateMode.IGNORE);
        ClientSettings.NicknameVisibility = new FusionPref<NicknameVisibility>(prefCategory, "Nickname Visibility", NicknameVisibility.SHOW_WITH_PREFIX, PrefUpdateMode.LOCAL_UPDATE);

        // Voicechat
        ClientSettings.Muted = new FusionPref<bool>(prefCategory, "Muted", false, PrefUpdateMode.IGNORE);
        ClientSettings.MutedIndicator = new FusionPref<bool>(prefCategory, "Muted Indicator", true, PrefUpdateMode.IGNORE);
        ClientSettings.Deafened = new FusionPref<bool>(prefCategory, "Deafened", false, PrefUpdateMode.IGNORE);
        ClientSettings.GlobalVolume = new FusionPref<float>(prefCategory, "GlobalMicVolume", 1f, PrefUpdateMode.IGNORE);

        // Gamemodes
        ClientSettings.GamemodeLateJoining = new FusionPref<bool>(prefCategory, "Gamemode Late Joining", true, PrefUpdateMode.IGNORE);

        // Save category
        prefCategory.SaveToFile(false);
    }

    internal static void OnPreferencesLoaded()
    {
        OnPrefsLoaded?.Invoke();
    }
}