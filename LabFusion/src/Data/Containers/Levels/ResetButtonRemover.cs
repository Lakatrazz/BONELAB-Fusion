using LabFusion.Network;

using UnityEngine;

using Il2CppSLZ.Marrow.VoidLogic;

namespace LabFusion.Data;

public sealed class ResetButtonRemover : LevelDataHandler
{
    // This should always apply to all levels.
    public override string LevelTitle => null;

    // List of all blacklisted names
    private static readonly string[] _blacklistedButtons = new string[] {
        "button_1_5x_Float_Powered_Entity_RESET",
        "button_1_5x_Float_Powered_Entity_HUB",
        "button_1x_Float_Floor",
    };

    protected override void MainSceneInitialized()
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Get all buttons
        var buttons = GameObject.FindObjectsOfType<ButtonNode>();

        for (var i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];

            var eventAdapter = button.GetComponentInChildren<EventAdapter>(true);

            if (eventAdapter == null)
            {
                continue;
            }

            // Get name
            string name = button.gameObject.name;
            string parentName = button.transform.parent ? button.transform.parent.gameObject.name : name;

            // Check if the name is blacklisted
            foreach (var blacklist in _blacklistedButtons)
            {
                bool inBlacklist = name.Contains(blacklist) || parentName.Contains(blacklist);

                if (!inBlacklist)
                {
                    continue;
                }

                eventAdapter.InputFell?.Clear();
                eventAdapter.InputHeld?.Clear();
                eventAdapter.InputRose?.Clear();
                eventAdapter.InputRoseOneShot?.Clear();
                eventAdapter.InputUpdated?.Clear();
                break;
            }
        }
    }
}