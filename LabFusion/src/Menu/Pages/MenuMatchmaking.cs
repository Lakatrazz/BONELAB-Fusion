using LabFusion.Marrow.Proxies;
using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuMatchmaking
{
    public static MenuPage MatchmakingPage { get; private set; }

    public static LobbyElement LobbyPanel { get; private set; }

    public static PageElement LobbyBrowserElement { get; private set; }
    public static PageElement SearchResultsElement { get; private set; }

    public static void PopulateMatchmaking(GameObject matchmakingPage)
    {
        MatchmakingPage = matchmakingPage.GetComponent<MenuPage>();

        LobbyPanel = matchmakingPage.transform.Find("page_Lobby/panel_Lobby").GetComponent<LobbyElement>();

        LobbyPanel.GetElements();

        LobbyPanel.Interactable = false;

        MatchmakingPage.DefaultPageIndex = 2;

        // Get browser references
        var browserTransform = matchmakingPage.transform.Find("page_Browser");

        PopulateBrowser(browserTransform);
    }
    
    private static void PopulateBrowser(Transform browserTransform)
    {
        LobbyBrowserElement = browserTransform.Find("scrollRect_LobbyBrowser/Viewport/Content").GetComponent<PageElement>();

        SearchResultsElement = LobbyBrowserElement.AddElement<PageElement>("Search Results");

        var searchBarElement = browserTransform.Find("button_SearchBar").GetComponent<FunctionElement>();

        var refreshElement = browserTransform.Find("button_Refresh").GetComponent<FunctionElement>();
        refreshElement.Do(RefreshBrowser);

        RefreshBrowser();
    }


    private static bool _isSearchingLobbies = false;

    public static void RefreshBrowser()
    {
        if (_isSearchingLobbies)
        {
            return;
        }

        SearchResultsElement.RemoveElements<LobbyResultElement>();

        _isSearchingLobbies = true;

        NetworkInfo.CurrentNetworkLayer.Matchmaker?.RequestLobbies(OnLobbiesRequested);
    }

    private static void OnLobbiesRequested(IMatchmaker.MatchmakerCallbackInfo info)
    {
        _isSearchingLobbies = false;

        foreach (var lobby in info.lobbies)
        {
            var lobbyResult = SearchResultsElement.AddElement<LobbyResultElement>("Lobby Result");
            var lobbyInfo = LobbyMetadataHelper.ReadInfo(lobby);

            ApplyLobbyToResult(lobbyResult, lobby, lobbyInfo);
        }
    }

    private static void ApplyLobbyToResult(LobbyResultElement element, INetworkLobby lobby, LobbyMetadataInfo info)
    {
        element.GetReferences();

        element.LevelNameText.text = info.LevelName;

        element.ServerNameText.text = ParseServerName(info.LobbyName, info.LobbyOwner);
        element.HostNameText.text = info.LobbyOwner;
        element.PlayerCountText.text = string.Format($"{info.PlayerCount}/{info.MaxPlayers} Players");
        element.VersionText.text = string.Format($"v{info.LobbyVersion}");

        var levelIcon = MenuResources.GetLevelIcon(info.LevelName);

        if (levelIcon == null)
        {
            levelIcon = MenuResources.GetLevelIcon(MenuResources.ModsIconTitle);
        }

        element.LevelIcon.texture = levelIcon;
    }

    private static string ParseServerName(string serverName, string hostName)
    {
        if (string.IsNullOrWhiteSpace(serverName))
        {
            serverName = $"{hostName}'s Server";
        }

        return serverName;
    }

    private static void ApplyServerMetadataToLobby(LobbyElement element, INetworkLobby lobby, LobbyMetadataInfo info)
    {
        var lobbyColor = Color.white;
        var levelColor = Color.white;
        var versionColor = Color.white;
        var playerCountColor = Color.white;

        if (!info.ClientHasLevel)
        {
            lobbyColor = Color.yellow;
            levelColor = Color.yellow;
        }

        if (NetworkVerification.CompareVersion(info.LobbyVersion, FusionMod.Version) != VersionResult.Ok)
        {
            lobbyColor = Color.red;
            versionColor = Color.red;
        }

        if (info.PlayerCount >= info.MaxPlayers)
        {
            lobbyColor = Color.red;
            playerCountColor = Color.red;
        }

        element.ServerActionElement
            .Cleared()
            .WithTitle("Join")
            .Do(info.CreateJoinDelegate(lobby))
            .WithColor(lobbyColor);

        element.LevelNameElement
            .WithTitle(info.LevelName)
            .WithColor(levelColor);

        element.ServerVersionElement
            .WithTitle($"v{info.LobbyVersion}")
            .WithColor(versionColor);

        element.PlayerCountElement
            .WithTitle($"{info.PlayerCount} Player{(info.PlayerCount != 1 ? "s" : "")}")
            .WithColor(playerCountColor);

        element.MaxPlayersElement
            .Cleared()
            .WithTitle("Max Players")
            .WithColor(playerCountColor);

        element.MaxPlayersElement.Value = info.MaxPlayers;

        element.PrivacyElement
            .Cleared()
            .WithTitle("Privacy");

        element.PrivacyElement.Value = info.Privacy;

        element.PrivacyElement.TextFormat = "{1}";

        element.ServerNameElement
            .Cleared()
            .WithTitle("Server Name");

        element.ServerNameElement.EmptyFormat = "No {0}";
        element.ServerNameElement.TextFormat = "{1}";

        element.ServerNameElement.Value = ParseServerName(info.LobbyName, info.LobbyOwner);

        element.HostNameElement
            .WithTitle(info.LobbyOwner);
    }
}
