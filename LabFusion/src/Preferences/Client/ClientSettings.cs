using LabFusion.Network;
using LabFusion.Senders;

using MelonLoader;

using UnityEngine;

namespace LabFusion.Preferences.Client;

public struct ClientSettings
{
    // Selected network layer
    public static FusionPref<string> NetworkLayerTitle { get; internal set; }
    public static FusionPref<int> ProxyPort { get; internal set; }

    // Nametag settings
    public static FusionPref<bool> NametagsEnabled { get; internal set; }
    public static FusionPref<Color> NametagColor { get; internal set; }

    // Nickname settings
    public static FusionPref<string> Nickname { get; internal set; }
    public static FusionPref<NicknameVisibility> NicknameVisibility { get; internal set; }

    // Voicechat settings
    public static FusionPref<bool> Muted { get; internal set; }
    public static FusionPref<bool> MutedIndicator { get; internal set; }
    public static FusionPref<bool> Deafened { get; internal set; }
    public static FusionPref<float> GlobalVolume { get; internal set; }

    // Gamemode settings
    public static FusionPref<bool> GamemodeLateJoining { get; internal set; }

    public static DownloadingSettings Downloading { get; private set; }

    public static void OnInitialize(MelonPreferences_Category category)
    {
        // Client settings
        NetworkLayerTitle = new FusionPref<string>(category, "Network Layer Title", NetworkLayerDeterminer.GetDefaultLayer().Title, PrefUpdateMode.IGNORE);
        ProxyPort = new FusionPref<int>(category, "Proxy Port", 28340, PrefUpdateMode.IGNORE);

        // Nametag
        NametagsEnabled = new FusionPref<bool>(category, "Client Nametags Enabled", true, PrefUpdateMode.LOCAL_UPDATE);
        NametagColor = new FusionPref<Color>(category, "Nametag Color", Color.white, PrefUpdateMode.CLIENT_UPDATE);

        // Nickname
        Nickname = new FusionPref<string>(category, "Nickname", "", PrefUpdateMode.IGNORE);
        NicknameVisibility = new FusionPref<NicknameVisibility>(category, "Nickname Visibility", Senders.NicknameVisibility.SHOW_WITH_PREFIX, PrefUpdateMode.LOCAL_UPDATE);

        // Voicechat
        Muted = new FusionPref<bool>(category, "Muted", false, PrefUpdateMode.IGNORE);
        MutedIndicator = new FusionPref<bool>(category, "Muted Indicator", true, PrefUpdateMode.IGNORE);
        Deafened = new FusionPref<bool>(category, "Deafened", false, PrefUpdateMode.IGNORE);
        GlobalVolume = new FusionPref<float>(category, "GlobalMicVolume", 1f, PrefUpdateMode.IGNORE);

        // Gamemodes
        GamemodeLateJoining = new FusionPref<bool>(category, "Gamemode Late Joining", true, PrefUpdateMode.IGNORE);

        Downloading = new DownloadingSettings();
        Downloading.CreatePrefs(category);
    }
}
