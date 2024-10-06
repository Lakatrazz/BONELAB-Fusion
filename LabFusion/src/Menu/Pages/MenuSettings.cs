using LabFusion.Marrow.Proxies;
using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuSettings
{
    public static void PopulateSettings(GameObject settingsPage)
    {
        var root = settingsPage.GetComponentInChildren<GroupElement>();

        // General settings
        var generalSettingsGroup = root.AddElement<GroupElement>("General Settings");

        generalSettingsGroup.AddElement<IntElement>("Max Players");
        generalSettingsGroup.AddElement<EnumElement>("Server Privacy").EnumType = typeof(ServerPrivacy);
        generalSettingsGroup.AddElement<BoolElement>("Nametags");
        generalSettingsGroup.AddElement<BoolElement>("Voice Chat");

        // Gameplay settings
        var gameplaySettingsGroup = root.AddElement<GroupElement>("Gameplay Settings");

        gameplaySettingsGroup.AddElement<BoolElement>("Server Mortality");
    }
}