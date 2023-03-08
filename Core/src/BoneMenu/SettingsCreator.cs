using BoneLib.BoneMenu.Elements;

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

        private static void CreateServerSettingsMenu(MenuCategory category)
        {
            // Nametags enabled
            CreateBoolPreference(category, "Nametags", FusionPreferences.LocalServerSettings.NametagsEnabled);

            // Voice chat
            CreateBoolPreference(category, "Voicechat", FusionPreferences.LocalServerSettings.VoicechatEnabled);

            // Server privacy
            CreateEnumPreference(category, "Server Privacy", FusionPreferences.LocalServerSettings.Privacy);

            // Time scale mode
            CreateEnumPreference(category, "Time Scale Mode", FusionPreferences.LocalServerSettings.TimeScaleMode);

            // Server mortality
            CreateBoolPreference(category, "Server Mortality", FusionPreferences.LocalServerSettings.ServerMortality);
            MultiplayerHooking.OnServerSettingsChanged += () => {
                // Update mortality
                if (Gamemode.ActiveGamemode == null)
                    FusionPlayer.ResetMortality();
            };

            // Max players
            CreateBytePreference(category, "Max Players", 1, 2, 255, FusionPreferences.LocalServerSettings.MaxPlayers);

            // Permissions
            var permissionCategory = category.CreateCategory("Permission Settings", Color.white);
            CreateEnumPreference(permissionCategory, "Dev Tools Allowed", FusionPreferences.LocalServerSettings.DevToolsAllowed);
            CreateEnumPreference(permissionCategory, "Kicking Allowed", FusionPreferences.LocalServerSettings.KickingAllowed);
            CreateEnumPreference(permissionCategory, "Banning Allowed", FusionPreferences.LocalServerSettings.BanningAllowed);
            CreateEnumPreference(permissionCategory, "Teleporation Allowed", FusionPreferences.LocalServerSettings.Teleportation);
        }

        private static void CreateClientSettingsMenu(MenuCategory category)
        {
            // Nametags enabled
            var nametagCategory = category.CreateCategory("Nametag Settings", Color.white);

            CreateBoolPreference(nametagCategory, "Nametags", FusionPreferences.ClientSettings.NametagsEnabled);

            // Nametag color
            CreateColorPreference(nametagCategory, FusionPreferences.ClientSettings.NametagColor);

            // Nickname
            var nicknameCategory = category.CreateCategory("Nickname Settings", Color.white);

            CreateEnumPreference(nicknameCategory, "Nickname Visibility", FusionPreferences.ClientSettings.NicknameVisibility);

            string currentNickname = PlayerIdManager.LocalNickname;
            var nickname = nicknameCategory.CreateFunctionElement(string.IsNullOrWhiteSpace(currentNickname) ? "No Nickname" : $"Nickname: {currentNickname}", Color.white, null);
            var pasteNickname = nicknameCategory.CreateFunctionElement("Paste Nickname", Color.white, () => {
                var text = Clipboard.GetText();
                FusionPreferences.ClientSettings.Nickname.SetValue(text);
            });
            var resetNickname = nicknameCategory.CreateFunctionElement("Reset Nickname", Color.white, () => {
                FusionPreferences.ClientSettings.Nickname.SetValue("");
            });

            FusionPreferences.ClientSettings.Nickname.OnValueChanged += (v) => {
                nickname.SetName(string.IsNullOrWhiteSpace(v) ? "No Nickname" : $"Nickname: {v}");

                if (PlayerIdManager.LocalId != null)
                    PlayerIdManager.LocalId.TrySetMetadata(MetadataHelper.NicknameKey, v);
            };

            // Voice chat
            var voiceChatCategory = category.CreateCategory("Voice Chat", Color.white);

            CreateBoolPreference(voiceChatCategory, "Muted", FusionPreferences.ClientSettings.Muted);
            CreateBoolPreference(voiceChatCategory, "Deafened", FusionPreferences.ClientSettings.Deafened);
            CreateFloatPreference(voiceChatCategory, "Global Volume", 0.1f, 0f, 10f, FusionPreferences.ClientSettings.GlobalVolume);
        }

    }
}
