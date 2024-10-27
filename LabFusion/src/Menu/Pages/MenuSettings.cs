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
            .AsPref(SavedServerSettings.NameTags);

        generalSettingsGroup.AddElement<BoolElement>("Voice Chat")
            .AsPref(SavedServerSettings.VoiceChat);

        // Gameplay
        var gameplaySettingsGroup = page.AddElement<GroupElement>("Gameplay");

        gameplaySettingsGroup.AddElement<EnumElement>("SlowMo")
            .AsPref(SavedServerSettings.SlowMoMode);

        gameplaySettingsGroup.AddElement<BoolElement>("Mortality")
            .AsPref(SavedServerSettings.Mortality);

        // Move this out of this class eventually
        LobbyInfoManager.OnLobbyInfoChanged += () =>
        {
            // Update mortality
            if (Gamemode.ActiveGamemode == null)
            {
                FusionPlayer.ResetMortality();
            }
        };

        gameplaySettingsGroup.AddElement<BoolElement>("Player Constraining")
            .AsPref(SavedServerSettings.PlayerConstraints);

        // Permissions
        var permissionsGroup = page.AddElement<GroupElement>("Permissions");

        permissionsGroup.AddElement<EnumElement>("Dev Tools")
            .AsPref(SavedServerSettings.DevTools);

        permissionsGroup.AddElement<EnumElement>("Constrainer")
            .AsPref(SavedServerSettings.Constrainer);

        permissionsGroup.AddElement<EnumElement>("Custom Avatars")
            .AsPref(SavedServerSettings.CustomAvatars);

        permissionsGroup.AddElement<EnumElement>("Kicking")
            .AsPref(SavedServerSettings.Kicking);

        permissionsGroup.AddElement<EnumElement>("Banning")
            .AsPref(SavedServerSettings.Banning);

        permissionsGroup.AddElement<EnumElement>("Teleportation")
            .AsPref(SavedServerSettings.Teleportation);
    }

    private static void PopulateClientSettings(PageElement page)
    {
        // Visual
        var visualGroup = page.AddElement<GroupElement>("Visuals");

        visualGroup.AddElement<BoolElement>("NameTags")
            .AsPref(ClientSettings.NameTags);

        visualGroup.AddElement<EnumElement>("Nickname Visibility")
            .AsPref(ClientSettings.NicknameVisibility);

        visualGroup.AddElement<BoolElement>("Mute Icon")
            .AsPref(ClientSettings.VoiceChat.MutedIndicator);

        // NameTag color
        var nameTagColorPref = ClientSettings.NameTagColor;

        Color.RGBToHSV(nameTagColorPref.Value, out var defaultH, out var defaultS, out var defaultV);

        var nameTagColorGroup = page.AddElement<GroupElement>("NameTag Color")
            .WithColor(nameTagColorPref.Value);

        var hueElement = nameTagColorGroup.AddElement<FloatElement>("Hue")
            .WithIncrement(0.05f)
            .WithLimits(0f, 1f);

        hueElement.Value = defaultH;

        var saturationElement = nameTagColorGroup.AddElement<FloatElement>("Saturation")
            .WithIncrement(0.05f)
            .WithLimits(0f, 1f);

        saturationElement.Value = defaultS;

        var valueElement = nameTagColorGroup.AddElement<FloatElement>("Value")
            .WithIncrement(0.05f)
            .WithLimits(0f, 1f);

        valueElement.Value = defaultV;

        hueElement.OnValueChanged += OnHueElementChanged;
        saturationElement.OnValueChanged += OnSaturationElementChanged;
        valueElement.OnValueChanged += OnValueElementChanged;

        nameTagColorPref.OnValueChanged += OnColorPreferenceChanged;

        nameTagColorGroup.OnCleared += OnColorElementCleared;

        void OnColorElementCleared()
        {
            nameTagColorPref.OnValueChanged -= OnColorPreferenceChanged;
        }

        void OnHueElementChanged(float hue)
        {
            Color.RGBToHSV(nameTagColorPref.Value, out _, out var s, out var v);

            nameTagColorPref.Value = Color.HSVToRGB(hue, s, v);
        }

        void OnSaturationElementChanged(float saturation)
        {
            Color.RGBToHSV(nameTagColorPref.Value, out var h, out _, out var v);

            nameTagColorPref.Value = Color.HSVToRGB(h, saturation, v);
        }

        void OnValueElementChanged(float value)
        {
            Color.RGBToHSV(nameTagColorPref.Value, out var h, out var s, out _);

            nameTagColorPref.Value = Color.HSVToRGB(h, s, value);
        }

        void OnColorPreferenceChanged(Color value)
        {
            Color.RGBToHSV(value, out var h, out var s, out var v);

            hueElement.OnValueChanged -= OnHueElementChanged;
            saturationElement.OnValueChanged -= OnSaturationElementChanged;
            valueElement.OnValueChanged -= OnValueElementChanged;

            hueElement.Value = h;
            saturationElement.Value = s;
            valueElement.Value = v;

            nameTagColorGroup.Color = value;

            hueElement.OnValueChanged += OnHueElementChanged;
            saturationElement.OnValueChanged += OnSaturationElementChanged;
            valueElement.OnValueChanged += OnValueElementChanged;
        }
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