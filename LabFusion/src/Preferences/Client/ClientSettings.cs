using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;

using MelonLoader;

using UnityEngine;

namespace LabFusion.Preferences.Client;

public static class ClientSettings
{
    // Selected network layer
    public static FusionPref<string> NetworkLayerTitle { get; internal set; }
    public static FusionPref<int> ProxyPort { get; internal set; }

    // Menu settings
    public static FusionPref<float> MenuSize { get; internal set; }

    // Nametag settings
    public static FusionPref<bool> NameTags { get; internal set; }

    public static Color NameTagColor => Color.HSVToRGB(NameTagHue.Value, NameTagSaturation.Value, NameTagValue.Value);

    public static FusionPref<float> NameTagHue { get; internal set; }
    public static FusionPref<float> NameTagSaturation { get; internal set; }
    public static FusionPref<float> NameTagValue { get; internal set; }

    // Nickname settings
    public static FusionPref<string> Nickname { get; internal set; }
    public static FusionPref<NicknameVisibility> NicknameVisibility { get; internal set; }

    // Description settings
    public static FusionPref<string> Description { get; internal set; }

    public static VoiceChatSettings VoiceChat { get; private set; }

    public static DownloadingSettings Downloading { get; private set; }

    public static SafetySettings Safety { get; private set; }

    public static void OnInitialize(MelonPreferences_Category category)
    {
        // Client settings
        NetworkLayerTitle = new FusionPref<string>(category, "Network Layer Title", NetworkLayerDeterminer.GetDefaultLayer().Title, PrefUpdateMode.IGNORE);
        ProxyPort = new FusionPref<int>(category, "Proxy Port", 28340, PrefUpdateMode.IGNORE);

        // Menu
        MenuSize = new FusionPref<float>(category, "Menu Size", 1f, PrefUpdateMode.IGNORE);

        // Nametag
        NameTags = new FusionPref<bool>(category, "Client Nametags Enabled", true, PrefUpdateMode.SERVER_UPDATE);

        NameTagHue = new FusionPref<float>(category, "NameTag Hue", 0f, PrefUpdateMode.CLIENT_UPDATE);
        NameTagSaturation = new FusionPref<float>(category, "NameTag Saturation", 0f, PrefUpdateMode.CLIENT_UPDATE);
        NameTagValue = new FusionPref<float>(category, "NameTag Value", 1f, PrefUpdateMode.CLIENT_UPDATE);

        // Nickname
        Nickname = new FusionPref<string>(category, "Nickname", string.Empty, PrefUpdateMode.IGNORE);
        NicknameVisibility = new FusionPref<NicknameVisibility>(category, "Nickname Visibility", Senders.NicknameVisibility.SHOW_WITH_PREFIX, PrefUpdateMode.SERVER_UPDATE);

        LocalPlayer.Metadata.Nickname.SetValue(Nickname.Value);

        Nickname.OnValueChanged += (v) =>
        {
            LocalPlayer.Metadata.Nickname.SetValue(v);
        };

        // Description
        Description = new FusionPref<string>(category, "Description", string.Empty, PrefUpdateMode.IGNORE);

        LocalPlayer.Metadata.Description.SetValue(Description.Value);

        Description.OnValueChanged += (v) =>
        {
            LocalPlayer.Metadata.Description.SetValue(v);
        };

        VoiceChat = new VoiceChatSettings();
        VoiceChat.CreatePrefs(category);

        Downloading = new DownloadingSettings();
        Downloading.CreatePrefs(category);

        Safety = new SafetySettings();
        Safety.CreatePrefs(category);
    }
}
