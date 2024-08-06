using LabFusion.Senders;

using UnityEngine;

namespace LabFusion.Preferences;

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
}
