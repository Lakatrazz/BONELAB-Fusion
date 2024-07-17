using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;
using LabFusion.Voice;

using UnityEngine;

using BoneLib.BoneMenu;

namespace LabFusion.BoneMenu;

public static partial class BoneMenuCreator
{
    // List of all possible tags
    private static readonly string[] _tagsList = new string[] {
        "Campaign",
        "Sandbox",
        "Roleplay",
        "Gamemode",
        "Hangout"
    };

    private static readonly FusionDictionary<string, BoolElement> _tagElements = new();

    // Settings menu
    private static Page _serverSettingsCategory;
    private static Page _clientSettingsCategory;

    public static void CreateSettingsMenu(Page page)
    {
        // Root settings
        var settings = page.CreatePage("Settings", Color.gray);

        // Server settings
        _serverSettingsCategory = settings.CreatePage("Server Settings", Color.white);
        CreateServerSettingsMenu(_serverSettingsCategory);

        // Client settings
        _clientSettingsCategory = settings.CreatePage("Client Settings", Color.white);
        CreateClientSettingsMenu(_clientSettingsCategory);
    }

    private static void CreateServerTagsMenu(Page page)
    {
        _tagElements.Clear();
        var tagsCategory = page.CreatePage("Server Tags", Color.white);
        tagsCategory.CreateFunction("Clear Tags", Color.white, () =>
        {
            FusionPreferences.LocalServerSettings.ServerTags.SetValue(new List<string>());
        });
        FusionPreferences.LocalServerSettings.ServerTags.OnValueChanged += (v) =>
        {
            foreach (var element in _tagElements.Values)
            {
                element.Value = false;
            }

            foreach (var tag in v)
            {
                if (_tagElements.TryGetValue(tag, out var element))
                {
                    element.Value = true;
                }
            }
        };

        var serverTags = FusionPreferences.LocalServerSettings.ServerTags.GetValue();

        foreach (var tag in _tagsList)
        {
            _tagElements.Add(tag, tagsCategory.CreateBool(tag, Color.white, serverTags.Contains(tag), (v) =>
            {
                // Refresh
                serverTags = FusionPreferences.LocalServerSettings.ServerTags.GetValue();

                // Add tag
                if (v)
                {
                    if (!serverTags.Contains(tag))
                        serverTags.Add(tag);

                    FusionPreferences.LocalServerSettings.ServerTags.SetValue(serverTags);
                }
                // Remove tag
                else
                {
                    serverTags.Remove(tag);

                    FusionPreferences.LocalServerSettings.ServerTags.SetValue(serverTags);
                }
            }));
        }
    }

    private static void CreateServerSettingsMenu(Page page)
    {
        // Cheat detection
        var cheatsCategory = page.CreatePage("Cheat Detection", Color.white);
        var statChangerSubPanel = cheatsCategory.CreatePage("Stat Changers", Color.white);
        CreateEnumPreference(statChangerSubPanel, "Stat Changers Allowed", FusionPreferences.LocalServerSettings.StatChangersAllowed);
        CreateFloatPreference(statChangerSubPanel, "Stat Changer Leeway", 1f, 0f, 10f, FusionPreferences.LocalServerSettings.StatChangerLeeway);

        // Server display
        var displaySettingsCategory = page.CreatePage("Display Settings", Color.white);
        CreateStringPreference(displaySettingsCategory, "Server Name", FusionPreferences.LocalServerSettings.ServerName);
        CreateServerTagsMenu(displaySettingsCategory);

        // General settings
        var generalSettingsSubPanel = page.CreatePage("General Settings", Color.white);
        CreateBytePreference(generalSettingsSubPanel, "Max Players", 1, 2, 255, FusionPreferences.LocalServerSettings.MaxPlayers);
        CreateEnumPreference(generalSettingsSubPanel, "Server Privacy", FusionPreferences.LocalServerSettings.Privacy);
        CreateBoolPreference(generalSettingsSubPanel, "Nametags", FusionPreferences.LocalServerSettings.NametagsEnabled);
        CreateBoolPreference(generalSettingsSubPanel, "Voicechat", FusionPreferences.LocalServerSettings.VoicechatEnabled);
        CreateBoolPreference(generalSettingsSubPanel, "Vote Kicking", FusionPreferences.LocalServerSettings.VoteKickingEnabled);

        // Gameplay settings
        var gameplaySettingsSubPanel = page.CreatePage("Gameplay Settings", Color.white);
        CreateEnumPreference(gameplaySettingsSubPanel, "Time Scale Mode", FusionPreferences.LocalServerSettings.TimeScaleMode);
        CreateBoolPreference(gameplaySettingsSubPanel, "Server Mortality", FusionPreferences.LocalServerSettings.ServerMortality);
        MultiplayerHooking.OnServerSettingsChanged += () =>
        {
            // Update mortality
            if (Gamemode.ActiveGamemode == null)
                FusionPlayer.ResetMortality();
        };
        CreateBoolPreference(gameplaySettingsSubPanel, "Player Constraining", FusionPreferences.LocalServerSettings.PlayerConstraintsEnabled);

        // Permissions
        var permissionSubPanel = page.CreatePage("Permission Settings", Color.white);
        CreateEnumPreference(permissionSubPanel, "Dev Tools Allowed", FusionPreferences.LocalServerSettings.DevToolsAllowed);
        CreateEnumPreference(permissionSubPanel, "Constrainer Allowed", FusionPreferences.LocalServerSettings.ConstrainerAllowed);
        CreateEnumPreference(permissionSubPanel, "Custom Avatars Allowed", FusionPreferences.LocalServerSettings.CustomAvatarsAllowed);
        CreateEnumPreference(permissionSubPanel, "Kicking Allowed", FusionPreferences.LocalServerSettings.KickingAllowed);
        CreateEnumPreference(permissionSubPanel, "Banning Allowed", FusionPreferences.LocalServerSettings.BanningAllowed);
        CreateEnumPreference(permissionSubPanel, "Teleporation Allowed", FusionPreferences.LocalServerSettings.Teleportation);

        // Platform discriminators
        var platformSubPanel = page.CreatePage("Platform Discrimination", Color.white);

        CreateBoolPreference(platformSubPanel, "Allow Quest Users", FusionPreferences.LocalServerSettings.AllowQuestUsers);
        CreateBoolPreference(platformSubPanel, "Allow PC Users", FusionPreferences.LocalServerSettings.AllowPCUsers);
    }

    private static void CreateClientSettingsMenu(Page page)
    {
        // Nametags enabled
        var nametagSubPanel = page.CreatePage("Nametag Settings", Color.white);

        CreateBoolPreference(nametagSubPanel, "Nametags", FusionPreferences.ClientSettings.NametagsEnabled);

        // Nametag color
        var color = FusionPreferences.ClientSettings.NametagColor.GetValue();
        color.a = 1f;
        FusionPreferences.ClientSettings.NametagColor.SetValue(color);

        CreateColorPreference(nametagSubPanel, FusionPreferences.ClientSettings.NametagColor);

        // Nickname
        var nicknameSubPanel = page.CreatePage("Nickname Settings", Color.white);

        CreateEnumPreference(nicknameSubPanel, "Nickname Visibility", FusionPreferences.ClientSettings.NicknameVisibility);

        CreateStringPreference(nicknameSubPanel, "Nickname", FusionPreferences.ClientSettings.Nickname, (v) =>
        {
            if (PlayerIdManager.LocalId != null)
                PlayerIdManager.LocalId.Metadata.TrySetMetadata(MetadataHelper.NicknameKey, v);
        });

        // Voice chat
        var voiceChatSubPanel = page.CreatePage("Voice Chat Settings", Color.white, 0, false);
        var voiceChatLink = page.CreatePageLink(voiceChatSubPanel);

        if (VoiceInfo.CanTalk)
        {
            CreateBoolPreference(voiceChatSubPanel, "Muted", FusionPreferences.ClientSettings.Muted);
            CreateBoolPreference(voiceChatSubPanel, "Muted Indicator", FusionPreferences.ClientSettings.MutedIndicator);
        }

        if (VoiceInfo.CanHear)
        {
            CreateBoolPreference(voiceChatSubPanel, "Deafened", FusionPreferences.ClientSettings.Deafened);
            CreateFloatPreference(voiceChatSubPanel, "Global Volume", 0.1f, 0f, 10f, FusionPreferences.ClientSettings.GlobalVolume);
        }

        RemoveEmptyPage(page, voiceChatSubPanel, voiceChatLink);
    }

}