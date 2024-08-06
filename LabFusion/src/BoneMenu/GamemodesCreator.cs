using BoneLib.BoneMenu;

using LabFusion.Preferences.Client;
using LabFusion.SDK.Gamemodes;

using UnityEngine;

namespace LabFusion.BoneMenu;

public static partial class BoneMenuCreator
{
    private static Page _gamemodesCategory;
    private static FunctionElement _gamemodeElement;
    private static FunctionElement _markedElement;

    public static void CreateGamemodesMenu(Page page)
    {
        // Root category
        _gamemodesCategory = page.CreatePage("Gamemodes", Color.cyan);
        ClearGamemodes();

        // Hook late joining change
        ClientSettings.GamemodeLateJoining.OnValueChanged += (v) =>
        {
            Gamemode.LateJoining = v;
        };

        Gamemode.LateJoining = ClientSettings.GamemodeLateJoining.Value;
    }

    public static void SetActiveGamemodeText(string text)
    {
        if (_gamemodeElement != null)
        {
            _gamemodeElement.ElementName = text;
        }
    }

    public static void SetMarkedGamemodeText(string text)
    {
        if (_markedElement != null)
        {
            _markedElement.ElementName = text;
        }
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
                var upperCategory = _gamemodesCategory.CreatePage(gamemode.GamemodeCategory, Color.white);
                var lowerCategory = upperCategory.CreatePage(gamemode.GamemodeName, Color.white);
                gamemode.OnBoneMenuCreated(lowerCategory);
            }
        }

        // Add stop button
        var activity = _gamemodesCategory.CreatePage("Activity", Color.white);
        _gamemodeElement = activity.CreateFunction("No Active Gamemode", Color.white, () =>
        {
            if (Gamemode.ActiveGamemode != null)
                Gamemode.ActiveGamemode.StopGamemode();
        });

        // Add marked button
        _markedElement = activity.CreateFunction("No Marked Gamemode", Color.white, () =>
        {
            if (Gamemode.MarkedGamemode != null)
                Gamemode.MarkedGamemode.UnmarkGamemode();
        });

        // Add toggle buttons
        var options = _gamemodesCategory.CreatePage("Options", Color.white);
        CreateBoolPreference(options, "Late Joining", ClientSettings.GamemodeLateJoining);
    }

    public static void ClearGamemodes(bool showText = true)
    {
        // Clear all gamemodes from the list
        _gamemodesCategory.RemoveAll();

        // Don't show the text if disabled
        if (!showText)
            return;

        // Add text for joining a server
        _gamemodesCategory.CreateFunction("Gamemodes will show when in a server!", Color.yellow, null);
    }
}