using BoneLib.BoneMenu.Elements;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator {
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
        private static MenuCategory _serverSettingsCategory;
        private static MenuCategory _clientSettingsCategory;

        public static void CreateSettingsMenu(MenuCategory category)
        {
            // Root settings
            var settings = category.CreateCategory("Settings", Color.gray);

            // Server settings
            _serverSettingsCategory = settings.CreateCategory("Server Settings", Color.white);
            CreateServerSettingsMenu(_serverSettingsCategory);

            // Client settings
            _clientSettingsCategory = settings.CreateCategory("Client Settings", Color.white);
            CreateClientSettingsMenu(_clientSettingsCategory);
        }

        private static void CreateServerTagsMenu(MenuCategory category) {
            var tagsCategory = category.CreateCategory("Server Tags", Color.white);
            tagsCategory.CreateFunctionElement("Clear Tags", Color.white, () => {
                FusionPreferences.LocalServerSettings.ServerTags.SetValue(new List<string>());
            });
            FusionPreferences.LocalServerSettings.ServerTags.OnValueChanged += (v) => {
                foreach (var element in _tagElements.Values) {
                    element.SetValue(false);
                }

                foreach (var tag in v) {
                    if (_tagElements.TryGetValue(tag, out var element)) {
                        element.SetValue(true);
                    }
                }
            };

            var serverTags = FusionPreferences.LocalServerSettings.ServerTags.GetValue();

            foreach (var tag in _tagsList) {
                _tagElements.Add(tag, tagsCategory.CreateBoolElement(tag, Color.white, serverTags.Contains(tag), (v) => {
                    // Refresh
                    serverTags = FusionPreferences.LocalServerSettings.ServerTags.GetValue();

                    // Add tag
                    if (v) {
                        if (!serverTags.Contains(tag))
                            serverTags.Add(tag);

                        FusionPreferences.LocalServerSettings.ServerTags.SetValue(serverTags);
                    }
                    // Remove tag
                    else {
                        serverTags.Remove(tag);

                        FusionPreferences.LocalServerSettings.ServerTags.SetValue(serverTags);
                    }
                }));
            }
        }

        private static void CreateServerSettingsMenu(MenuCategory category)
        {
            // Cheat detection
            var cheatsCategory = category.CreateCategory("Cheat Detection", Color.white);
            var statChangerSubPanel = cheatsCategory.CreateSubPanel("Stat Changers", Color.white);
            CreateEnumPreference(statChangerSubPanel, "Stat Changers Allowed", FusionPreferences.LocalServerSettings.StatChangersAllowed);
            CreateFloatPreference(statChangerSubPanel, "Stat Changer Leeway", 1f, 0f, 10f, FusionPreferences.LocalServerSettings.StatChangerLeeway);

            // Server display
            var displaySettingsCategory = category.CreateCategory("Display Settings", Color.white);
            CreateStringPreference(displaySettingsCategory, "Server Name", FusionPreferences.LocalServerSettings.ServerName);
            CreateServerTagsMenu(displaySettingsCategory);

            // General settings
            var generalSettingsSubPanel = category.CreateSubPanel("General Settings", Color.white);
            CreateBytePreference(generalSettingsSubPanel, "Max Players", 1, 2, 255, FusionPreferences.LocalServerSettings.MaxPlayers);
            CreateEnumPreference(generalSettingsSubPanel, "Server Privacy", FusionPreferences.LocalServerSettings.Privacy);
            CreateBoolPreference(generalSettingsSubPanel, "Nametags", FusionPreferences.LocalServerSettings.NametagsEnabled);
            CreateBoolPreference(generalSettingsSubPanel, "Voicechat", FusionPreferences.LocalServerSettings.VoicechatEnabled);
            CreateBoolPreference(generalSettingsSubPanel, "Vote Kicking", FusionPreferences.LocalServerSettings.VoteKickingEnabled);
            
            // Gameplay settings
            var gameplaySettingsSubPanel = category.CreateSubPanel("Gameplay Settings", Color.white);
            CreateEnumPreference(gameplaySettingsSubPanel, "Time Scale Mode", FusionPreferences.LocalServerSettings.TimeScaleMode);
            CreateBoolPreference(gameplaySettingsSubPanel, "Server Mortality", FusionPreferences.LocalServerSettings.ServerMortality);
            MultiplayerHooking.OnServerSettingsChanged += () => {
                // Update mortality
                if (Gamemode.ActiveGamemode == null)
                    FusionPlayer.ResetMortality();
            };
            CreateBoolPreference(gameplaySettingsSubPanel, "Player Constraining", FusionPreferences.LocalServerSettings.PlayerConstraintsEnabled);
            
            // Permissions
            var permissionSubPanel = category.CreateSubPanel("Permission Settings", Color.white);
            CreateEnumPreference(permissionSubPanel, "Dev Tools Allowed", FusionPreferences.LocalServerSettings.DevToolsAllowed);
            CreateEnumPreference(permissionSubPanel, "Constrainer Allowed", FusionPreferences.LocalServerSettings.ConstrainerAllowed);
            CreateEnumPreference(permissionSubPanel, "Custom Avatars Allowed", FusionPreferences.LocalServerSettings.CustomAvatarsAllowed);
            CreateEnumPreference(permissionSubPanel, "Kicking Allowed", FusionPreferences.LocalServerSettings.KickingAllowed);
            CreateEnumPreference(permissionSubPanel, "Banning Allowed", FusionPreferences.LocalServerSettings.BanningAllowed);
            CreateEnumPreference(permissionSubPanel, "Teleporation Allowed", FusionPreferences.LocalServerSettings.Teleportation);

            // Platform discriminators
            var platformSubPanel = category.CreateSubPanel("Platform Discrimination", Color.white);

            CreateBoolPreference(platformSubPanel, "Allow Quest Users", FusionPreferences.LocalServerSettings.AllowQuestUsers);
            CreateBoolPreference(platformSubPanel, "Allow PC Users", FusionPreferences.LocalServerSettings.AllowPCUsers);
        }

        private static void CreateClientSettingsMenu(MenuCategory category)
        {
            // Nametags enabled
            var nametagSubPanel = category.CreateSubPanel("Nametag Settings", Color.white);

            CreateBoolPreference(nametagSubPanel, "Nametags", FusionPreferences.ClientSettings.NametagsEnabled);

            // Nametag color
            var color = FusionPreferences.ClientSettings.NametagColor.GetValue();
            color.a = 1f;
            FusionPreferences.ClientSettings.NametagColor.SetValue(color);

            CreateColorPreference(nametagSubPanel, FusionPreferences.ClientSettings.NametagColor);

            // Nickname
            var nicknameSubPanel = category.CreateSubPanel("Nickname Settings", Color.white);

            CreateEnumPreference(nicknameSubPanel, "Nickname Visibility", FusionPreferences.ClientSettings.NicknameVisibility);

            CreateStringPreference(nicknameSubPanel, "Nickname", FusionPreferences.ClientSettings.Nickname, (v) => {
                if (PlayerIdManager.LocalId != null)
                    PlayerIdManager.LocalId.TrySetMetadata(MetadataHelper.NicknameKey, v);
            });

            // Voice chat
            var voiceChatSubPanel = category.CreateSubPanel("Voice Chat Settings", Color.white);

            CreateBoolPreference(voiceChatSubPanel, "Muted", FusionPreferences.ClientSettings.Muted);
            CreateBoolPreference(voiceChatSubPanel, "Muted Indicator", FusionPreferences.ClientSettings.MutedIndicator);
            CreateBoolPreference(voiceChatSubPanel, "Deafened", FusionPreferences.ClientSettings.Deafened);
            CreateFloatPreference(voiceChatSubPanel, "Global Volume", 0.1f, 0f, 10f, FusionPreferences.ClientSettings.GlobalVolume);
        }

    }
}
