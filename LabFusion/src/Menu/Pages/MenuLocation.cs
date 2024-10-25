using LabFusion.Downloading.ModIO;
using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Server;
using LabFusion.Scene;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuLocation
{
    public static MenuPage LocationPage { get; private set; }

    public static LobbyElement LobbyElement { get; private set; }

    public static void OnInitializeMelon()
    {
        MultiplayerHooking.OnStartServer += OnConnect;
        MultiplayerHooking.OnJoinServer += OnConnect;
        MultiplayerHooking.OnDisconnect += OnDisconnect;

        ServerSettingsManager.OnServerSettingsChanged += OnServerSettingsChanged;
    }

    private static void OnConnect()
    {
        if (LobbyElement == null)
        {
            return;
        }

        PopulateLobbyAsServer(LobbyElement);
    }

    private static void OnDisconnect()
    {
        if (LobbyElement == null)
        {
            return;
        }

        PopulateLobbyNoServer(LobbyElement);
    }

    private static void PopulateLobbyNoServer(LobbyElement element)
    {
        element.ServerActionElement
            .Cleared()
            .WithTitle("Create Server")
            .Do(NetworkHelper.StartServer);

        UpdateLevelIcon(element);

        OnServerSettingsChanged();
    }

    private static void PopulateLobbyAsServer(LobbyElement element)
    {
        element.ServerActionElement
            .Cleared()
            .WithTitle("Disconnect")
            .Do(() => { NetworkHelper.Disconnect(); });

        UpdateLevelIcon(element);

        OnServerSettingsChanged();
    }

    private static void UpdateLevelIcon(LobbyElement element)
    {
        var levelName = FusionSceneManager.Title;

        var levelIcon = MenuResources.GetLevelIcon(levelName);

        if (levelIcon == null)
        {
            levelIcon = MenuResources.GetLevelIcon(MenuResources.ModsIconTitle);
        }

        element.LevelIcon.texture = levelIcon;

        if (!FusionSceneManager.Level.Pallet.IsInMarrowGame())
        {
            ApplyModTexture(element);
        }
    }

    private static void ApplyModTexture(LobbyElement element)
    {
        // Get the mod info
        var pallet = FusionSceneManager.Level.Pallet;

        var manifest = CrateFilterer.GetManifest(pallet);

        if (manifest == null)
        {
            return;
        }

        var modListing = manifest.ModListing;

        var modTarget = ModIOManager.GetTargetFromListing(modListing);

        if (modTarget == null)
        {
            return;
        }

        // Get the texture
        ModIOThumbnailDownloader.GetThumbnail((int)modTarget.ModId, (texture) =>
        {
            element.LevelIcon.texture = texture;
        });
    }

    private static void OnServerSettingsChanged()
    {
        ApplyServerSettingsToLobby(LobbyElement, ServerSettingsManager.ActiveSettings);
    }

    private static void ApplyServerSettingsToLobby(LobbyElement element, ServerSettings settings)
    {
        bool ownsSettings = NetworkInfo.IsServer || !NetworkInfo.HasServer;

        string emptyFormat = ownsSettings ? "Click to add {0}" : "No {0}";

        element.LevelNameElement
            .WithTitle(FusionSceneManager.Title);

        element.ServerVersionElement
            .WithTitle($"v{FusionMod.Version}");

        var playerCount = PlayerIdManager.PlayerCount;

        element.PlayersElement
            .Cleared()
            .AsPref(settings.MaxPlayers)
            .WithIncrement(1)
            .WithLimits(2, 255)
            .WithTitle("Players");

        element.PlayersElement.TextFormat = $"{playerCount}/{{1}} {{0}}";

        element.PrivacyElement
            .Cleared()
            .AsPref(settings.Privacy)
            .WithTitle("Privacy");

        element.PrivacyElement.TextFormat = "{1}";

        element.ServerNameElement
            .Cleared()
            .AsPref(settings.ServerName)
            .WithTitle("Server Name");

        element.ServerNameElement.EmptyFormat = emptyFormat;
        element.ServerNameElement.TextFormat = "{1}";

        element.HostNameElement
            .WithTitle($"{PlayerIdManager.LocalUsername}");

        element.DescriptionElement
            .Cleared()
            .AsPref(settings.ServerDescription)
            .WithTitle("Description");

        element.DescriptionElement.EmptyFormat = emptyFormat;
        element.DescriptionElement.TextFormat = "{1}";

        element.MoreElement
            .Cleared()
            .WithTitle("More...")
            .Do(() => { element.LobbyPage.SelectSubPage(1); });

        // Fill out lists
        // Settings list
        var settingsPage = element.SettingsElement.Pages[0];

        var generalGroup = settingsPage.AddOrGetElement<GroupElement>("General");

        generalGroup.AddOrGetElement<BoolElement>("NameTags")
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(settings.NametagsEnabled)
            .WithTitle("NameTags");

        generalGroup.AddOrGetElement<BoolElement>("VoiceChat")
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(settings.VoiceChatEnabled)
            .WithTitle("VoiceChat");

        generalGroup.AddOrGetElement<EnumElement>("SlowMo")
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(settings.TimeScaleMode)
            .WithTitle("SlowMo");

        // Player list
        element.PlayerBrowserElement.Clear();

        var playerListPage = element.PlayerBrowserElement.AddPage();

        foreach (var player in PlayerIdManager.PlayerIds)
        {
            MetadataHelper.TryGetDisplayName(player, out var name);

            var playerResult = playerListPage.AddElement<PlayerResultElement>(name);

            playerResult.GetReferences();

            playerResult.PlayerNameText.text = name;

            playerResult.RoleText.text = "User";
        }

        // Change interactability for all elements
        element.Interactable = ownsSettings;
    }

    public static void PopulateLocation(GameObject locationPage)
    {
        LocationPage = locationPage.GetComponent<MenuPage>();

        LobbyElement = locationPage.transform.Find("panel_Lobby").GetComponent<LobbyElement>();

        LobbyElement.GetElements();

        LobbyElement.SettingsElement.AddPage();

        // Update server status
        if (NetworkInfo.HasServer)
        {
            OnConnect();
        }
        else
        {
            OnDisconnect();
        }
    }
}