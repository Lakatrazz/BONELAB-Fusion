using LabFusion.Marrow.Proxies;

using LabFusion.Preferences;
using LabFusion.Preferences.Client;

using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuSettings
{
    public static void PopulateSettings(GameObject settingsPage)
    {
        var rootPage = settingsPage.transform.Find("scrollRect_Options/Viewport/Content").GetComponent<PageElement>();

        var serverPage = rootPage.AddPage("Server");

        PopulateServerSettings(serverPage);

        var clientPage = rootPage.AddPage("Client");

        PopulateClientSettings(clientPage);

        // Categories
        var categoriesRoot = settingsPage.transform.Find("scrollRect_Categories/Viewport/Content").GetComponent<PageElement>();
        var categoriesPage = categoriesRoot.AddPage("Default");

        categoriesPage.AddElement<FunctionElement>("Server").Link(serverPage);
        categoriesPage.AddElement<FunctionElement>("Client").Link(clientPage);
    }

    private static void PopulateServerSettings(PageElement page)
    {
        // General
        var generalSettingsGroup = page.AddElement<GroupElement>("General");

        generalSettingsGroup.AddElement<IntElement>("Max Players")
            .AsPref(ServerSettingsManager.SavedSettings.MaxPlayers)
            .WithIncrement(1)
            .WithLimits(2, 255);

        generalSettingsGroup.AddElement<EnumElement>("Server Privacy")
            .AsPref(ServerSettingsManager.SavedSettings.Privacy);

        generalSettingsGroup.AddElement<BoolElement>("NameTags")
            .AsPref(ServerSettingsManager.SavedSettings.NametagsEnabled);

        generalSettingsGroup.AddElement<BoolElement>("Voice Chat")
            .AsPref(ServerSettingsManager.SavedSettings.VoiceChatEnabled);

        // Gameplay
        var gameplaySettingsGroup = page.AddElement<GroupElement>("Gameplay");

        gameplaySettingsGroup.AddElement<EnumElement>("Time Scale Mode")
            .AsPref(ServerSettingsManager.SavedSettings.TimeScaleMode);

        gameplaySettingsGroup.AddElement<BoolElement>("Server Mortality")
            .AsPref(ServerSettingsManager.SavedSettings.ServerMortality);

        // Move this out of this class eventually
        MultiplayerHooking.OnServerSettingsChanged += () =>
        {
            // Update mortality
            if (Gamemode.ActiveGamemode == null)
            {
                FusionPlayer.ResetMortality();
            }
        };

        gameplaySettingsGroup.AddElement<BoolElement>("Player Constraining")
            .AsPref(ServerSettingsManager.SavedSettings.PlayerConstraintsEnabled);

        // Permissions
        var permissionsGroup = page.AddElement<GroupElement>("Permissions");

        permissionsGroup.AddElement<EnumElement>("Dev Tools Allowed")
            .AsPref(ServerSettingsManager.SavedSettings.DevToolsAllowed);

        permissionsGroup.AddElement<EnumElement>("Constrainer Allowed")
            .AsPref(ServerSettingsManager.SavedSettings.ConstrainerAllowed);

        permissionsGroup.AddElement<EnumElement>("Custom Avatars Allowed")
            .AsPref(ServerSettingsManager.SavedSettings.CustomAvatarsAllowed);

        permissionsGroup.AddElement<EnumElement>("Kicking Allowed")
            .AsPref(ServerSettingsManager.SavedSettings.KickingAllowed);

        permissionsGroup.AddElement<EnumElement>("Banning Allowed")
            .AsPref(ServerSettingsManager.SavedSettings.BanningAllowed);

        permissionsGroup.AddElement<EnumElement>("Teleportation Allowed")
            .AsPref(ServerSettingsManager.SavedSettings.Teleportation);
    }

    private static void PopulateClientSettings(PageElement page)
    {
        // Visual
        var visualGroup = page.AddElement<GroupElement>("Visuals");

        visualGroup.AddElement<BoolElement>("NameTags")
            .AsPref(ClientSettings.NametagsEnabled);

        visualGroup.AddElement<EnumElement>("Nickname Visibility")
            .AsPref(ClientSettings.NicknameVisibility);

        visualGroup.AddElement<BoolElement>("Mute Icon")
            .AsPref(ClientSettings.VoiceChat.MutedIndicator);
    }
}