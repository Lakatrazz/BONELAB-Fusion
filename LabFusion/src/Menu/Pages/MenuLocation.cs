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
            .Clear()
            .WithTitle("Create Server")
            .Do(NetworkHelper.StartServer);

        AssignLobbyToLocalSettings(element);
    }

    private static void PopulateLobbyAsServer(LobbyElement element)
    {
        element.ServerActionElement
            .Clear()
            .WithTitle("Disconnect")
            .Do(() => { NetworkHelper.Disconnect(); });

        AssignLobbyToLocalSettings(element);
    }

    private static void AssignLobbyToLocalSettings(LobbyElement element)
    {
        element.LevelNameElement
            .WithTitle(FusionSceneManager.Title);

        element.ServerVersionElement
            .WithTitle($"v{FusionMod.Version}");

        var playerCount = PlayerIdManager.PlayerCount;

        element.PlayerCountElement
            .WithTitle($"{playerCount} Player{(playerCount != 1 ? "s" : "")}");

        element.MaxPlayersElement
            .Clear()
            .AsPref(ServerSettingsManager.SavedSettings.MaxPlayers)
            .WithIncrement(1)
            .WithLimits(2, 255)
            .WithTitle("Max Players");

        element.PrivacyElement
            .Clear()
            .AsPref(ServerSettingsManager.SavedSettings.Privacy)
            .WithTitle("Privacy");

        element.ServerNameElement
            .Clear()
            .AsPref(ServerSettingsManager.SavedSettings.ServerName)
            .WithTitle("Server Name");

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