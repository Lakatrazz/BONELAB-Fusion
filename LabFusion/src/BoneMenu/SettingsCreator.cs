using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Preferences.Client;
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
            ServerSettingsManager.SavedSettings.ServerTags.Value = new List<string>();
        });
        ServerSettingsManager.SavedSettings.ServerTags.OnValueChanged += (v) =>
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

        var serverTags = ServerSettingsManager.SavedSettings.ServerTags.Value;

        foreach (var tag in _tagsList)
        {
            _tagElements.Add(tag, tagsCategory.CreateBool(tag, Color.white, serverTags.Contains(tag), (v) =>
            {
                // Refresh
                serverTags = ServerSettingsManager.SavedSettings.ServerTags.Value;

                // Add tag
                if (v)
                {
                    if (!serverTags.Contains(tag))
                        serverTags.Add(tag);

                    ServerSettingsManager.SavedSettings.ServerTags.Value = serverTags;
                }
                // Remove tag
                else
                {
                    serverTags.Remove(tag);

                    ServerSettingsManager.SavedSettings.ServerTags.Value = serverTags;
                }
            }));
        }
    }

    private static void CreateServerSettingsMenu(Page page)
    {
        // Cheat detection
        var cheatsCategory = page.CreatePage("Cheat Detection", Color.white);
        var statChangerSubPanel = cheatsCategory.CreatePage("Stat Changers", Color.white);
        CreateEnumPreference(statChangerSubPanel, "Stat Changers Allowed", ServerSettingsManager.SavedSettings.StatChangersAllowed);
        CreateFloatPreference(statChangerSubPanel, "Stat Changer Leeway", 1f, 0f, 10f, ServerSettingsManager.SavedSettings.StatChangerLeeway);

        // Server display
        var displaySettingsCategory = page.CreatePage("Display Settings", Color.white);
        CreateStringPreference(displaySettingsCategory, "Server Name", ServerSettingsManager.SavedSettings.ServerName);
        CreateServerTagsMenu(displaySettingsCategory);

        // General settings
        var generalSettingsSubPanel = page.CreatePage("General Settings", Color.white);
        CreateBytePreference(generalSettingsSubPanel, "Max Players", 1, 2, 255, ServerSettingsManager.SavedSettings.MaxPlayers);
        CreateEnumPreference(generalSettingsSubPanel, "Server Privacy", ServerSettingsManager.SavedSettings.Privacy);
        CreateBoolPreference(generalSettingsSubPanel, "Nametags", ServerSettingsManager.SavedSettings.NametagsEnabled);
        CreateBoolPreference(generalSettingsSubPanel, "Voice Chat", ServerSettingsManager.SavedSettings.VoicechatEnabled);
        CreateBoolPreference(generalSettingsSubPanel, "Vote Kicking", ServerSettingsManager.SavedSettings.VoteKickingEnabled);

        // Gameplay settings
        var gameplaySettingsSubPanel = page.CreatePage("Gameplay Settings", Color.white);
        CreateEnumPreference(gameplaySettingsSubPanel, "Time Scale Mode", ServerSettingsManager.SavedSettings.TimeScaleMode);
        CreateBoolPreference(gameplaySettingsSubPanel, "Server Mortality", ServerSettingsManager.SavedSettings.ServerMortality);
        MultiplayerHooking.OnServerSettingsChanged += () =>
        {
            // Update mortality
            if (Gamemode.ActiveGamemode == null)
                FusionPlayer.ResetMortality();
        };
        CreateBoolPreference(gameplaySettingsSubPanel, "Player Constraining", ServerSettingsManager.SavedSettings.PlayerConstraintsEnabled);

        // Permissions
        var permissionSubPanel = page.CreatePage("Permission Settings", Color.white);
        CreateEnumPreference(permissionSubPanel, "Dev Tools Allowed", ServerSettingsManager.SavedSettings.DevToolsAllowed);
        CreateEnumPreference(permissionSubPanel, "Constrainer Allowed", ServerSettingsManager.SavedSettings.ConstrainerAllowed);
        CreateEnumPreference(permissionSubPanel, "Custom Avatars Allowed", ServerSettingsManager.SavedSettings.CustomAvatarsAllowed);
        CreateEnumPreference(permissionSubPanel, "Kicking Allowed", ServerSettingsManager.SavedSettings.KickingAllowed);
        CreateEnumPreference(permissionSubPanel, "Banning Allowed", ServerSettingsManager.SavedSettings.BanningAllowed);
        CreateEnumPreference(permissionSubPanel, "Teleportation Allowed", ServerSettingsManager.SavedSettings.Teleportation);

        // Platform discriminators
        var platformSubPanel = page.CreatePage("Platform Discrimination", Color.white);

        CreateBoolPreference(platformSubPanel, "Allow Quest Users", ServerSettingsManager.SavedSettings.AllowQuestUsers);
        CreateBoolPreference(platformSubPanel, "Allow PC Users", ServerSettingsManager.SavedSettings.AllowPCUsers);
    }

    private static void CreateClientSettingsMenu(Page page)
    {
        // Nametags enabled
        var nametagSubPanel = page.CreatePage("Nametag Settings", Color.white);

        CreateBoolPreference(nametagSubPanel, "Nametags", ClientSettings.NametagsEnabled);

        // Nametag color
        var color = ClientSettings.NametagColor.Value;
        color.a = 1f;
        ClientSettings.NametagColor.Value = color;

        CreateColorPreference(nametagSubPanel, ClientSettings.NametagColor);

        // Nickname
        var nicknameSubPanel = page.CreatePage("Nickname Settings", Color.white);

        CreateEnumPreference(nicknameSubPanel, "Nickname Visibility", ClientSettings.NicknameVisibility);

        CreateStringPreference(nicknameSubPanel, "Nickname", ClientSettings.Nickname, (v) =>
        {
            if (PlayerIdManager.LocalId != null)
                PlayerIdManager.LocalId.Metadata.TrySetMetadata(MetadataHelper.NicknameKey, v);
        });

        // Voice chat
        var voiceChatSubPanel = page.CreatePage("Voice Chat Settings", Color.white, 0, false);
        var voiceChatLink = page.CreatePageLink(voiceChatSubPanel);

        if (VoiceInfo.CanTalk)
        {
            CreateBoolPreference(voiceChatSubPanel, "Muted", ClientSettings.Muted);
            CreateBoolPreference(voiceChatSubPanel, "Muted Indicator", ClientSettings.MutedIndicator);
        }

        if (VoiceInfo.CanHear)
        {
            CreateBoolPreference(voiceChatSubPanel, "Deafened", ClientSettings.Deafened);
            CreateFloatPreference(voiceChatSubPanel, "Global Volume", 0.1f, 0f, 10f, ClientSettings.GlobalVolume);
        }

        RemoveEmptyPage(page, voiceChatSubPanel, voiceChatLink);
    }

}
