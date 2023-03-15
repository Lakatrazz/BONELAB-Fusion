using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;
using LabFusion.Preferences;
using LabFusion.SDK.Gamemodes;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator
    {
        private static MenuCategory _gamemodesCategory;
        private static FunctionElement _gamemodeElement;

        public static void CreateGamemodesMenu(MenuCategory category) {
            // Root category
            _gamemodesCategory = category.CreateCategory("Gamemodes", Color.cyan);
            ClearGamemodes();

            // Hook music enabled change
            FusionPreferences.ClientSettings.GamemodeMusic.OnValueChanged += (v) => {
                Gamemode.MusicToggled = v;
            };

            Gamemode.MusicToggled = FusionPreferences.ClientSettings.GamemodeMusic.GetValue();

            // Hook late joining change
            FusionPreferences.ClientSettings.GamemodeLateJoining.OnValueChanged += (v) => {
                Gamemode.LateJoining = v;
            };

            Gamemode.LateJoining = FusionPreferences.ClientSettings.GamemodeLateJoining.GetValue();
        }

        public static void SetActiveGamemodeText(string text) {
            if (_gamemodeElement != null)
                _gamemodeElement.SetName(text);
        }

        public static void RefreshGamemodes() {
            // Clear existing gamemodes just incase
            ClearGamemodes(false);

            // Add stop button
            _gamemodeElement = _gamemodesCategory.CreateFunctionElement("No Active Gamemode", Color.white, () =>
            {
                if (Gamemode.ActiveGamemode != null)
                    Gamemode.ActiveGamemode.StopGamemode();
            });

            // Add toggle buttons
            CreateBoolPreference(_gamemodesCategory, "Music", FusionPreferences.ClientSettings.GamemodeMusic);
            CreateBoolPreference(_gamemodesCategory, "Late Joining", FusionPreferences.ClientSettings.GamemodeLateJoining);

            // Add necessary gamemodes
            foreach (var gamemode in GamemodeManager.Gamemodes) {
                // Make sure the gamemode isnt null
                if (gamemode == null)
                    continue;

                // Make sure this gamemode should be in bonemenu
                if (gamemode.VisibleInBonemenu) {
                    var upperCategory = _gamemodesCategory.CreateCategory(gamemode.GamemodeCategory, Color.white);
                    var lowerCategory = upperCategory.CreateCategory(gamemode.GamemodeName, Color.white);
                    gamemode.OnBoneMenuCreated(lowerCategory);
                }
            }
        }

        public static void ClearGamemodes(bool showText = true) {
            // Clear all gamemodes from the list
            _gamemodesCategory.Elements.Clear();

            // Don't show the text if disabled
            if (!showText)
                return;

            // Add text for joining a server
            _gamemodesCategory.CreateFunctionElement("Gamemodes will show when in a server!", Color.yellow, null);
        }
    }
}
