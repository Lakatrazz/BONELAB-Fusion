using BoneLib.BoneMenu;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences;
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

        UpdateInteractability();
    }

    private static void PopulateLobbyAsServer(LobbyElement element)
    {
        element.ServerActionElement
            .Cleared()
            .WithTitle("Disconnect")
            .Do(() => { NetworkHelper.Disconnect(); });

        UpdateLevelIcon(element);

        OnServerSettingsChanged();

        UpdateInteractability();
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
    }

    private static void OnServerSettingsChanged()
    {
        ApplyServerSettingsToLobby(LobbyElement, ServerSettingsManager.ActiveSettings);
    }

    private static void UpdateInteractability()
    {
        bool ownsSettings = NetworkInfo.IsServer || !NetworkInfo.HasServer;

        LobbyElement.Interactable = ownsSettings;
    }

    private static void ApplyServerSettingsToLobby(LobbyElement element, ServerSettings settings)
    {
        element.LevelNameElement
            .WithTitle(FusionSceneManager.Title);

        element.ServerVersionElement
            .WithTitle($"v{FusionMod.Version}");

        var playerCount = PlayerIdManager.PlayerCount;

        element.PlayerCountElement
            .WithTitle($"{playerCount} Player{(playerCount != 1 ? "s" : "")}");

        element.MaxPlayersElement
            .Cleared()
            .AsPref(settings.MaxPlayers)
            .WithIncrement(1)
            .WithLimits(2, 255)
            .WithTitle("Max Players");

        element.PrivacyElement
            .Cleared()
            .AsPref(settings.Privacy)
            .WithTitle("Privacy");

        element.PrivacyElement.TextFormat = "{1}";

        element.ServerNameElement
            .Cleared()
            .AsPref(settings.ServerName)
            .WithTitle("Server Name");

        element.ServerNameElement.EmptyFormat = "No {0}";
        element.ServerNameElement.TextFormat = "{1}";

        element.HostNameElement
            .WithTitle($"{PlayerIdManager.LocalUsername}");
    }

    public static void PopulateLocation(GameObject locationPage)
    {
        LocationPage = locationPage.GetComponent<MenuPage>();

        LobbyElement = locationPage.transform.Find("panel_Lobby").GetComponent<LobbyElement>();

        LobbyElement.GetElements();

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