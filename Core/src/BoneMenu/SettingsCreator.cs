using BoneLib.BoneMenu.Elements;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;

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
            var nametags = category.CreateBoolElement("Nametags", Color.white, FusionPreferences.ServerSettings.NametagsEnabled, (v) => {
                FusionPreferences.ServerSettings.NametagsEnabled.SetValue(v);
            });
            FusionPreferences.ServerSettings.NametagsEnabled.OnValueChanged += (v) => {
                nametags.SetValue(v);
            };

            // Server privacy
            var privacy = category.CreateEnumElement<ServerPrivacy>("Server Privacy", Color.white, FusionPreferences.ServerSettings.Privacy, (v) => {
                FusionPreferences.ServerSettings.Privacy.SetValue(v);
            });
            FusionPreferences.ServerSettings.Privacy.OnValueChanged += (v) => {
                privacy.SetValue(v);
            };

            // Time scale mode
            var timeScale = category.CreateEnumElement<TimeScaleMode>("Time Scale Mode", Color.white, FusionPreferences.ServerSettings.TimeScaleMode, (v) => {
                FusionPreferences.ServerSettings.TimeScaleMode.SetValue(v);
            });
            FusionPreferences.ServerSettings.TimeScaleMode.OnValueChanged += (v) => {
                timeScale.SetValue(v);
            };
        }

        private static void CreateClientSettingsMenu(MenuCategory category)
        {
            // Nametags enabled
            var nametagCategory = category.CreateCategory("Nametag Settings", Color.white);

            var nametags = nametagCategory.CreateBoolElement("Nametags", Color.white, FusionPreferences.ClientSettings.NametagsEnabled, (v) => {
                FusionPreferences.ClientSettings.NametagsEnabled.SetValue(v);
            });
            FusionPreferences.ClientSettings.NametagsEnabled.OnValueChanged += (v) => {
                nametags.SetValue(v);
            };

            // Nametag color
            var currentColor = FusionPreferences.ClientSettings.NametagColor;
            var colorR = nametagCategory.CreateFloatElement("Red", Color.red, currentColor.GetValue().r, 0.05f, 0f, 1f, (r) => {
                var color = currentColor.GetValue();
                color.r = r;
                currentColor.SetValue(color);
            });
            var colorG = nametagCategory.CreateFloatElement("Green", Color.green, currentColor.GetValue().g, 0.05f, 0f, 1f, (g) => {
                var color = currentColor.GetValue();
                color.g = g;
                currentColor.SetValue(color);
            });
            var colorB = nametagCategory.CreateFloatElement("Blue", Color.blue, currentColor.GetValue().b, 0.05f, 0f, 1f, (b) => {
                var color = currentColor.GetValue();
                color.b = b;
                currentColor.SetValue(color);
            });
            var colorPreview = nametagCategory.CreateFunctionElement("■■■■■■■■■■■", currentColor, null);

            currentColor.OnValueChanged += (color) => {
                colorR.SetValue(color.r);
                colorG.SetValue(color.g);
                colorB.SetValue(color.b);
                colorPreview.SetColor(color);
            };

            // Nickname
            var nicknameCategory = category.CreateCategory("Nickname Settings", Color.white);

            var visibility = nicknameCategory.CreateEnumElement<NicknameVisibility>("Nickname Visibility", Color.white, FusionPreferences.ClientSettings.NicknameVisibility, (v) => {
                FusionPreferences.ClientSettings.NicknameVisibility.SetValue(v);
            });
            FusionPreferences.ClientSettings.NicknameVisibility.OnValueChanged += (v) =>
            {
                visibility.SetValue(v);
            };

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
        }

    }
}
