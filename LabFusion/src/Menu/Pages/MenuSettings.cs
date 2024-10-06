using LabFusion.Marrow.Proxies;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuSettings
{
    public static void PopulateSettings(GameObject settingsPage)
    {
        var root = settingsPage.GetComponentInChildren<MenuGroup>();

        // General settings
        var generalSettingsGroup = root.AddGroup("General Settings");

        generalSettingsGroup.AddInt("Max Players", 10, 1, 1, 255);
        generalSettingsGroup.AddBool("Nametags", true);
        generalSettingsGroup.AddBool("Voice Chat", true);

        // Gameplay settings
        var gameplaySettingsGroup = root.AddGroup("Gameplay Settings");

        gameplaySettingsGroup.AddBool("Server Mortality", true);
    }
}