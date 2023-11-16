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
        private static FunctionElement _markedElement;

        public static void CreateGamemodesMenu(MenuCategory category)
        {
            // Root category
            _gamemodesCategory = category.CreateCategory("Gamemodes", Color.cyan);
            ClearGamemodes();

            // Hook music enabled change
            FusionPreferences.ClientSettings.GamemodeMusic.OnValueChanged += (v) =>
            {
                Gamemode.MusicToggled = v;
            };

            Gamemode.MusicToggled = FusionPreferences.ClientSettings.GamemodeMusic.GetValue();

            // Hook late joining change
            FusionPreferences.ClientSettings.GamemodeLateJoining.OnValueChanged += (v) =>
            {
                Gamemode.LateJoining = v;
            };

            Gamemode.LateJoining = FusionPreferences.ClientSettings.GamemodeLateJoining.GetValue();
        }

        public static void SetActiveGamemodeText(string text)
        {
            if (_gamemodeElement != null)
                _gamemodeElement.SetName(text);
        }

        public static void SetMarkedGamemodeText(string text)
        {
            if (_markedElement != null)
                _markedElement.SetName(text);
        }

        public static void RefreshGamemodes()
        {
            // Clear existing gamemodes just incase
            ClearGamemodes(false);

            // Add necessary gamemodes
            foreach (var gamemode in GamemodeManager.Gamemodes)
            {
                // Make sure the gamemode isnt null
                if (gamemode == null)
                    continue;

                // Make sure this gamemode should be in bonemenu
                if (gamemode.VisibleInBonemenu)
                {
                    var upperCategory = _gamemodesCategory.CreateCategory(gamemode.GamemodeCategory, Color.white);
                    var lowerCategory = upperCategory.CreateCategory(gamemode.GamemodeName, Color.white);
                    gamemode.OnBoneMenuCreated(lowerCategory);
                }
            }

            // Add stop button
            var activity = _gamemodesCategory.CreateSubPanel("Activity", Color.white);
            _gamemodeElement = activity.CreateFunctionElement("No Active Gamemode", Color.white, () =>
            {
                if (Gamemode.ActiveGamemode != null)
                    Gamemode.ActiveGamemode.StopGamemode();
            });

            // Add marked button
            _markedElement = activity.CreateFunctionElement("No Marked Gamemode", Color.white, () =>
            {
                if (Gamemode.MarkedGamemode != null)
                    Gamemode.MarkedGamemode.UnmarkGamemode();
            });

            // Add toggle buttons
            var options = _gamemodesCategory.CreateSubPanel("Options", Color.white);
            CreateBoolPreference(options, "Music", FusionPreferences.ClientSettings.GamemodeMusic);
            CreateBoolPreference(options, "Late Joining", FusionPreferences.ClientSettings.GamemodeLateJoining);
        }

        public static void ClearGamemodes(bool showText = true)
        {
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
