using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.SDK.Lobbies;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuMatchmaking
{
    public static MenuPage MatchmakingPage { get; private set; }

    // Filters
    public static PageElement BrowserFiltersElement { get; private set; }

    public static PageElement SandboxFiltersPageElement { get; private set; }

    public static GroupElement SandboxFiltersGroupElement { get; private set; }

    // Browser
    public static PageElement LobbyBrowserElement { get; private set; }
    public static PageElement SearchResultsElement { get; private set; }

    public static StringElement SearchBarElement { get; private set; }

    public static LabelElement NoServersFoundElement { get; private set; }

    // Lobby
    public static LobbyElement LobbyPanel { get; private set; }

    public static void PopulateMatchmaking(GameObject matchmakingPage)
    {
        MatchmakingPage = matchmakingPage.GetComponent<MenuPage>();

        LobbyPanel = matchmakingPage.transform.Find("page_Lobby/panel_Lobby").GetComponent<LobbyElement>();

        LobbyPanel.GetElements();

        LobbyPanel.Interactable = false;

        // Get options references
        var optionsTransform = matchmakingPage.transform.Find("page_Options");

        PopulateOptions(optionsTransform);

        // Get browser references
        var browserTransform = matchmakingPage.transform.Find("page_Browser");

        PopulateBrowser(browserTransform);
    }
    
    private static void PopulateOptions(Transform optionsTransform)
    {
        var grid = optionsTransform.Find("grid_Options");

        var gamemodeElement = grid.Find("button_Gamemode").GetComponent<FunctionElement>();

        gamemodeElement.transform.Find("label_Title").GetComponent<LabelElement>().Title = "Gamemode";

        var sandboxElement = grid.Find("button_Sandbox").GetComponent<FunctionElement>();
        sandboxElement.Do(() =>
        {
            MatchmakingPage.SelectSubPage(4);

            RefreshBrowser();
        });

        sandboxElement.transform.Find("label_Title").GetComponent<LabelElement>().Title = "Sandbox";

        var codeElement = grid.Find("button_Code").GetComponent<FunctionElement>();

        codeElement.transform.Find("label_Title").GetComponent<LabelElement>().Title = "Enter Code";
    }

    private static void PopulateBrowser(Transform browserTransform)
    {
        // Get the filters group
        var filtersGroup = browserTransform.Find("group_Filters");

        BrowserFiltersElement = filtersGroup.Find("scrollRect_Filters/Viewport/Content").GetComponent<PageElement>();

        SandboxFiltersPageElement = BrowserFiltersElement.AddPage();
        SandboxFiltersGroupElement = SandboxFiltersPageElement.AddElement<GroupElement>("Filters");

        AddFilters();

        // Get the searching group
        var searchGroup = browserTransform.Find("group_Search");

        LobbyBrowserElement = searchGroup.Find("scrollRect_LobbyBrowser/Viewport/Content").GetComponent<PageElement>();

        SearchResultsElement = LobbyBrowserElement.AddElement<PageElement>("Search Results");

        SearchBarElement = searchGroup.Find("button_SearchBar").GetComponent<StringElement>();
        SearchBarElement.EmptyFormat = "Enter server or level name";
        SearchBarElement.TextFormat = "{1}";

        SearchBarElement.OnSubmitted += RefreshBrowser;

        NoServersFoundElement = searchGroup.Find("label_NoServersFound").GetComponent<LabelElement>();
        NoServersFoundElement.Title = "No servers found :/";

        var refreshElement = searchGroup.Find("button_Refresh").GetComponent<FunctionElement>();
        refreshElement.Do(RefreshBrowser);
    }

    public static void AddFilters()
    {
        foreach (var filter in LobbyFilterManager.LobbyFilters)
        {
            AddFilter(SandboxFiltersGroupElement, filter);
        }
    }

    private static void AddFilter(GroupElement element, ILobbyFilter filter)
    {
        var filterElement = element.AddElement<BoolElement>(filter.GetTitle());
        filterElement.Value = filter.IsActive();
        filterElement.OnValueChanged += (v) =>
        {
            filter.SetActive(v);

            RefreshBrowser();
        };
    }

    private static bool _isSearchingLobbies = false;

    public static void RefreshBrowser()
    {
        if (_isSearchingLobbies)
        {
            return;
        }

        SearchResultsElement.RemoveElements<LobbyResultElement>();

        NoServersFoundElement.gameObject.SetActive(false);

        _isSearchingLobbies = true;

        NetworkInfo.CurrentNetworkLayer.Matchmaker?.RequestLobbies(OnLobbiesRequested);
    }

    private static bool CheckLobbyVisibility(IMatchmaker.LobbyInfo info)
    {
        switch (info.metadata.Privacy)
        {
            case ServerPrivacy.PUBLIC:
                return true;
            case ServerPrivacy.FRIENDS_ONLY:
                return NetworkInfo.CurrentNetworkLayer.IsFriend(info.metadata.LobbyId);
            default:
                return false;
        }
    }

    private static bool CheckLobbySearch(IMatchmaker.LobbyInfo info, string query)
    {
        var metadata = info.metadata;
        var levelName = metadata.LevelName.ToLower();
        var serverName = metadata.LobbyName.ToLower();
        var hostName = metadata.LobbyOwner.ToLower();

        if (levelName.Contains(query))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(serverName) && serverName.Contains(query))
        {
            return true;
        }

        if (hostName.Contains(query))
        {
            return true;
        }

        return false;
    }

    private static void OnLobbiesRequested(IMatchmaker.MatchmakerCallbackInfo info)
    {
        _isSearchingLobbies = false;

        var sortedLobbies = info.lobbies
            .OrderBy(l => l.metadata.LobbyOwner)
            .OrderBy(l => l.metadata.LevelName)
            .OrderBy(l => l.metadata.LobbyVersion)
            .Where(CheckLobbyVisibility)
            .Where(l => LobbyFilterManager.FilterLobby(l.lobby, l.metadata));

        // Apply search query
        var searchQuery = SearchBarElement.Value;

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            sortedLobbies = sortedLobbies.Where((l) => CheckLobbySearch(l, searchQuery));
        }

        // Add all lobbies to the list
        foreach (var lobby in sortedLobbies)
        {
            var lobbyResult = SearchResultsElement.AddElement<LobbyResultElement>("Lobby Result");

            ApplyLobbyToResult(lobbyResult, lobby);
        }

        bool foundLobbies = sortedLobbies.Any();
        NoServersFoundElement.gameObject.SetActive(!foundLobbies);
    }

    private static void OnShowLobby(IMatchmaker.LobbyInfo info)
    {
        MatchmakingPage.SelectSubPage(5);

        ApplyServerMetadataToLobby(LobbyPanel, info.lobby, info.metadata);
    }

    private static void ApplyLobbyToResult(LobbyResultElement element, IMatchmaker.LobbyInfo info)
    {
        element.GetReferences();

        var metadata = info.metadata;

        element.OnPressed = () =>
        {
            OnShowLobby(info);
        };

        element.LevelNameText.text = metadata.LevelName;

        element.ServerNameText.text = ParseServerName(metadata.LobbyName, metadata.LobbyOwner);
        element.HostNameText.text = metadata.LobbyOwner;
        element.PlayerCountText.text = string.Format($"{metadata.PlayerCount}/{metadata.MaxPlayers} Players");
        element.VersionText.text = string.Format($"v{metadata.LobbyVersion}");

        var levelIcon = MenuResources.GetLevelIcon(metadata.LevelName);

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

        element.PlayersElement
            .Cleared()
            .WithTitle("Players")
            .WithColor(playerCountColor);

        element.PlayersElement.TextFormat = $"{info.PlayerCount}/{{1}} {{0}}";

        element.PlayersElement.Value = info.MaxPlayers;

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

        element.DescriptionElement
            .Cleared()
            .WithTitle("Description");

        element.DescriptionElement.EmptyFormat = "No {0}";
        element.DescriptionElement.TextFormat = "{1}";

        element.DescriptionElement.Value = info.LobbyDescription;

        // Apply level icon
        var levelIcon = MenuResources.GetLevelIcon(info.LevelName);

        if (levelIcon == null)
        {
            levelIcon = MenuResources.GetLevelIcon(MenuResources.ModsIconTitle);
        }

        element.LevelIcon.texture = levelIcon;
    }
}
