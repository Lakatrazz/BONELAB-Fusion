using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Preferences.Server;
using LabFusion.Preferences.Client;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuSettings
{
    public static void PopulateSettings(GameObject settingsPage)
    {
        var rootPage = settingsPage.transform.Find("scrollRect_Options/Viewport/Content").GetComponent<PageElement>();

        var serverPage = rootPage.AddPage();

        PopulateServerSettings(serverPage);

        var clientPage = rootPage.AddPage();

        PopulateClientSettings(clientPage);

        var downloadingPage = rootPage.AddPage();

        PopulateDownloadingSettings(downloadingPage);

        var networkLayerPage = rootPage.AddPage();

        PopulateNetworkLayerSettings(networkLayerPage);

#if DEBUG
        var debugPage = rootPage.AddPage();

        PopulateDebugSettings(debugPage);
#endif

        // Categories
        var categoriesRoot = settingsPage.transform.Find("scrollRect_Categories/Viewport/Content").GetComponent<PageElement>();
        var categoriesPage = categoriesRoot.AddPage("Default");

        categoriesPage.AddElement<FunctionElement>("Server").Link(serverPage).WithColor(Color.white);
        categoriesPage.AddElement<FunctionElement>("Client").Link(clientPage).WithColor(Color.white);
        categoriesPage.AddElement<FunctionElement>("Downloading").Link(downloadingPage).WithColor(Color.cyan);
        categoriesPage.AddElement<FunctionElement>("Network Layer").Link(networkLayerPage).WithColor(Color.yellow);

#if DEBUG
        categoriesPage.AddElement<FunctionElement>("Debug").Link(debugPage).WithColor(Color.red);
#endif
    }

    private static void PopulateServerSettings(PageElement page)
    {
        // General
        var generalSettingsGroup = page.AddElement<GroupElement>("General");

        generalSettingsGroup.AddElement<BoolElement>("NameTags")
            .AsPref(ServerSettingsManager.SavedSettings.NametagsEnabled);

        generalSettingsGroup.AddElement<BoolElement>("Voice Chat")
            .AsPref(ServerSettingsManager.SavedSettings.VoiceChatEnabled);

        // Gameplay
        var gameplaySettingsGroup = page.AddElement<GroupElement>("Gameplay");

        gameplaySettingsGroup.AddElement<EnumElement>("SlowMo")
            .AsPref(ServerSettingsManager.SavedSettings.TimeScaleMode);

        gameplaySettingsGroup.AddElement<BoolElement>("Mortality")
            .AsPref(ServerSettingsManager.SavedSettings.ServerMortality);

        // Move this out of this class eventually
        ServerSettingsManager.OnServerSettingsChanged += () =>
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

        permissionsGroup.AddElement<EnumElement>("Dev Tools")
            .AsPref(ServerSettingsManager.SavedSettings.DevToolsAllowed);

        permissionsGroup.AddElement<EnumElement>("Constrainer")
            .AsPref(ServerSettingsManager.SavedSettings.ConstrainerAllowed);

        permissionsGroup.AddElement<EnumElement>("Custom Avatars")
            .AsPref(ServerSettingsManager.SavedSettings.CustomAvatarsAllowed);

        permissionsGroup.AddElement<EnumElement>("Kicking")
            .AsPref(ServerSettingsManager.SavedSettings.KickingAllowed);

        permissionsGroup.AddElement<EnumElement>("Banning")
            .AsPref(ServerSettingsManager.SavedSettings.BanningAllowed);

        permissionsGroup.AddElement<EnumElement>("Teleportation")
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

    private static void PopulateDownloadingSettings(PageElement page)
    {
        var generalGroup = page.AddElement<GroupElement>("General");

        generalGroup.AddElement<BoolElement>("Download Spawnables")
            .AsPref(ClientSettings.Downloading.DownloadSpawnables);

        generalGroup.AddElement<BoolElement>("Download Avatars")
            .AsPref(ClientSettings.Downloading.DownloadAvatars);

        generalGroup.AddElement<BoolElement>("Download Levels")
            .AsPref(ClientSettings.Downloading.DownloadLevels);

        generalGroup.AddElement<BoolElement>("Keep Downloaded Mods")
            .AsPref(ClientSettings.Downloading.KeepDownloadedMods);

        generalGroup.AddElement<IntElement>("Max File Size (MB)")
            .AsPref(ClientSettings.Downloading.MaxFileSize)
            .WithIncrement(10)
            .WithLimits(0, 10000);

        generalGroup.AddElement<IntElement>("Max Level Size (MB)")
            .AsPref(ClientSettings.Downloading.MaxLevelSize)
            .WithIncrement(10)
            .WithLimits(0, 10000);
    }

    private static int _lastLayerIndex;

    private static void PopulateNetworkLayerSettings(PageElement page)
    {
        _lastLayerIndex = NetworkLayer.SupportedLayers.IndexOf(NetworkLayerDeterminer.LoadedLayer);

        var changeLayerGroup = page.AddElement<GroupElement>("Change Layer");

        changeLayerGroup.AddElement<FunctionElement>($"Active Layer: {NetworkLayerDeterminer.LoadedTitle}");

        var targetLayerElement = changeLayerGroup.AddElement<FunctionElement>($"Target Layer: {ClientSettings.NetworkLayerTitle.Value}");
        
        changeLayerGroup.AddElement<FunctionElement>("Cycle Layer")
            .Do(() =>
            {
                int count = NetworkLayer.SupportedLayers.Count;
                if (count <= 0)
                    return;

                _lastLayerIndex++;
                if (count <= _lastLayerIndex)
                    _lastLayerIndex = 0;

                ClientSettings.NetworkLayerTitle.Value = NetworkLayer.SupportedLayers[_lastLayerIndex].Title;
            });

        ClientSettings.NetworkLayerTitle.OnValueChanged += (v) =>
        {
            targetLayerElement.Title = $"Target Layer: {v}";
        };

        changeLayerGroup.AddElement<FunctionElement>("Apply Layer")
            .Do(() => InternalLayerHelpers.UpdateLoadedLayer());
    }

#if DEBUG
    private static void PopulateDebugSettings(PageElement page)
    {
        var generalGroup = page.AddElement<GroupElement>("General");

        generalGroup.AddElement<FunctionElement>("Spawn Player Rep")
            .Do(() =>
            {
                PlayerRepUtilities.CreateNewRig((rig) =>
                {
                    rig.transform.position = RigData.Refs.RigManager.physicsRig.feet.transform.position;
                });
            });

        generalGroup.AddElement<FunctionElement>("Send To Floating Point")
            .Do(() =>
            {
                var physRig = RigData.Refs.RigManager.physicsRig;

                float force = 100000000000000f;

                for (var i = 0; i < 10; i++)
                {
                    physRig.rbFeet.AddForce(Vector3Extensions.left * force, ForceMode.VelocityChange);
                    physRig.rbKnee.AddForce(Vector3Extensions.right * force, ForceMode.VelocityChange);
                    physRig.rightHand.rb.AddForce(Vector3Extensions.up * force, ForceMode.VelocityChange);
                    physRig.leftHand.rb.AddForce(Vector3Extensions.down * force, ForceMode.VelocityChange);
                }
            });
    }
#endif
}