using LabFusion.Data;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Lobbies;
using LabFusion.Senders;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuMatchmaking
{
    public static MenuPage MatchmakingPage { get; private set; }

    // Filters
    public static PageElement BrowserFiltersElement { get; private set; }

    public static PageElement SandboxFiltersPageElement { get; private set; }

    public static GroupElement SandboxFiltersGroupElement { get; private set; }

    // Code
    public static StringElement CodeElement { get; private set; }

    // Browser
    public static MenuPage BrowserPage { get; private set; }

    public static PageElement LobbyBrowserElement { get; private set; }
    public static PageElement SearchResultsElement { get; private set; }

    public static StringElement SearchBarElement { get; private set; }

    public static LabelElement NoServersFoundElement { get; private set; }

    // Lobby
    public static LobbyElement LobbyPanel { get; private set; }

    public static void PopulateMatchmaking(GameObject matchmakingPage)
    {
        MatchmakingPage = matchmakingPage.GetComponent<MenuPage>();

        // Get options references
        var optionsTransform = matchmakingPage.transform.Find("page_Options");

        PopulateOptions(optionsTransform);

        // Get code references
        var codeTransform = matchmakingPage.transform.Find("page_Code");

        PopulateCode(codeTransform);

        // Get browser references
        var browserTransform = matchmakingPage.transform.Find("page_Browser");

        PopulateBrowser(browserTransform);
    }
    
    private static void PopulateOptions(Transform optionsTransform)
    {
        var grid = optionsTransform.Find("grid_Options");

        var gamemodeElement = grid.Find("button_Gamemode").GetComponent<FunctionElement>();

        gamemodeElement.transform.Find("label_Title").GetComponent<LabelElement>().Title = "Gamemode";
        gamemodeElement.Do(() =>
        {
            MatchmakingPage.SelectSubPage(1);
        });

        var sandboxElement = grid.Find("button_Sandbox").GetComponent<FunctionElement>();
        sandboxElement.Do(() =>
        {
            MatchmakingPage.SelectSubPage(4);

            RefreshBrowser();
        });

        sandboxElement.transform.Find("label_Title").GetComponent<LabelElement>().Title = "Sandbox";

        var codeElement = grid.Find("button_Code").GetComponent<FunctionElement>();
        codeElement.Do(() =>
        {
            MatchmakingPage.SelectSubPage(3);
        });

        codeElement.transform.Find("label_Title").GetComponent<LabelElement>().Title = "Enter Code";
    }

    private static void PopulateCode(Transform codeTransform)
    {
        var grid = codeTransform.Find("grid_Buttons");

        CodeElement = grid.Find("button_Code").GetComponent<StringElement>();
        CodeElement.Title = "Code";
        CodeElement.EmptyFormat = "Enter {0}";

        CodeElement.OnKeyboardToggled += (v) =>
        {
            if (v)
            {
                var keyboard = MenuCreator.MenuPopups.Keyboard;
                keyboard.TemporaryUppercase = false;
                keyboard.Uppercase = true;
            }
        };

        var joinElement = grid.Find("button_Join").GetComponent<FunctionElement>();

        joinElement
            .WithTitle("Join")
            .Do(() =>
            {
                var code = CodeElement.Value;

                if (string.IsNullOrWhiteSpace(code))
                {
                    return;
                }

                NetworkHelper.JoinServerByCode(code);
            });
    }

    private static void PopulateBrowser(Transform browserTransform)
    {
        BrowserPage = browserTransform.GetComponent<MenuPage>();

        // Search page
        var searchPage = browserTransform.Find("page_Search");

        // Get the filters group
        var filtersGroup = searchPage.Find("group_Filters");

        BrowserFiltersElement = filtersGroup.Find("scrollRect_Filters/Viewport/Content").GetComponent<PageElement>();

        SandboxFiltersPageElement = BrowserFiltersElement.AddPage();
        SandboxFiltersGroupElement = SandboxFiltersPageElement.AddElement<GroupElement>("Filters");

        AddFilters();

        // Get the searching group
        var searchGroup = searchPage.Find("group_Search");

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

        // Lobby page
        LobbyPanel = browserTransform.Find("page_Lobby/panel_Lobby").GetComponent<LobbyElement>();

        LobbyPanel.GetElements();

        LobbyPanel.Interactable = false;
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
        switch (info.metadata.LobbyInfo.Privacy)
        {
            case ServerPrivacy.PUBLIC:
                return true;
            case ServerPrivacy.FRIENDS_ONLY:
                return NetworkInfo.CurrentNetworkLayer.IsFriend(info.metadata.LobbyInfo.LobbyId);
            default:
                return false;
        }
    }

    private static bool CheckLobbySearch(IMatchmaker.LobbyInfo info, string query)
    {
        var metadata = info.metadata;
        var levelName = metadata.LobbyInfo.LevelTitle.ToLower();
        var serverName = metadata.LobbyInfo.LobbyName.ToLower();
        var hostName = metadata.LobbyInfo.LobbyHostName.ToLower();

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
            .OrderBy(l => l.metadata.LobbyInfo.LobbyHostName)
            .OrderBy(l => l.metadata.LobbyInfo.LevelTitle)
            .OrderBy(l => l.metadata.LobbyInfo.LobbyVersion)
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
        MatchmakingPage.SelectSubPage(4);
        BrowserPage.SelectSubPage(1);

        ApplyServerMetadataToLobby(LobbyPanel, info.lobby, info.metadata);
    }

    private static void OnShowPlayer(PlayerInfo info)
    {
        MatchmakingPage.SelectSubPage(4);
        BrowserPage.SelectSubPage(1);
        LobbyPanel.LobbyPage.SelectSubPage(2);

        ApplyPlayerToElement(LobbyPanel.ProfileElement, info);
    }

    private static void ApplyLobbyToResult(LobbyResultElement element, IMatchmaker.LobbyInfo info)
    {
        element.GetReferences();

        var metadata = info.metadata;

        element.OnPressed = () =>
        {
            OnShowLobby(info);
        };

        var levelColor = Color.white;
        var versionColor = Color.white;
        var playerCountColor = Color.white;

        if (!metadata.ClientHasLevel)
        {
            levelColor = Color.yellow;
        }

        if (NetworkVerification.CompareVersion(metadata.LobbyInfo.LobbyVersion, FusionMod.Version) != VersionResult.Ok)
        {
            versionColor = Color.red;
        }

        if (metadata.LobbyInfo.PlayerCount >= metadata.LobbyInfo.MaxPlayers)
        {
            playerCountColor = Color.red;
        }

        element.LevelNameText.text = metadata.LobbyInfo.LevelTitle;
        element.LevelNameText.color = levelColor;

        element.ServerNameText.text = ParseServerName(metadata.LobbyInfo.LobbyName, metadata.LobbyInfo.LobbyHostName);
        element.HostNameText.text = metadata.LobbyInfo.LobbyHostName;

        element.PlayerCountText.text = string.Format($"{metadata.LobbyInfo.PlayerCount}/{metadata.LobbyInfo.MaxPlayers} Players");
        element.PlayerCountText.color = playerCountColor;

        element.VersionText.text = string.Format($"v{metadata.LobbyInfo.LobbyVersion}");
        element.VersionText.color = versionColor;

        ElementIconHelper.SetLevelResultIcon(element, metadata.LobbyInfo.LevelTitle);
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

        if (NetworkVerification.CompareVersion(info.LobbyInfo.LobbyVersion, FusionMod.Version) != VersionResult.Ok)
        {
            lobbyColor = Color.red;
            versionColor = Color.red;
        }

        if (info.LobbyInfo.PlayerCount >= info.LobbyInfo.MaxPlayers)
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
            .WithTitle(info.LobbyInfo.LevelTitle)
            .WithColor(levelColor);

        element.ServerVersionElement
            .WithTitle($"v{info.LobbyInfo.LobbyVersion}")
            .WithColor(versionColor);

        element.PlayersElement
            .Cleared()
            .WithTitle("Players")
            .WithColor(playerCountColor);

        element.PlayersElement.TextFormat = $"{info.LobbyInfo.PlayerCount}/{{1}} {{0}}";

        element.PlayersElement.Value = info.LobbyInfo.MaxPlayers;

        element.PrivacyElement
            .Cleared()
            .WithTitle("Privacy");

        element.PrivacyElement.Value = info.LobbyInfo.Privacy;

        element.PrivacyElement.TextFormat = "{1}";

        element.ServerNameElement
            .Cleared()
            .WithTitle("Server Name");

        element.ServerNameElement.EmptyFormat = "No {0}";
        element.ServerNameElement.TextFormat = "{1}";

        element.ServerNameElement.Value = ParseServerName(info.LobbyInfo.LobbyName, info.LobbyInfo.LobbyHostName);

        element.HostNameElement
            .WithTitle(info.LobbyInfo.LobbyHostName);

        element.DescriptionElement
            .Cleared()
            .WithTitle("Description");

        element.DescriptionElement.EmptyFormat = "No {0}";
        element.DescriptionElement.TextFormat = "{1}";

        element.DescriptionElement.Value = info.LobbyInfo.LobbyDescription;

        element.MoreElement
            .Cleared()
            .WithTitle("More...")
            .Do(() => { element.LobbyPage.SelectSubPage(1); });

        // Apply level icon
        ElementIconHelper.SetLevelIcon(element, info.LobbyInfo.LevelTitle);

        // Fill out lists
        // Settings list
        element.SettingsElement.Clear();

        var settingsPage = element.SettingsElement.AddPage();

        var generalGroup = settingsPage.AddElement<GroupElement>("General");

        generalGroup.AddElement<BoolElement>("NameTags")
            .WithInteractability(false)
            .Value = info.LobbyInfo.NameTags;

        generalGroup.AddElement<BoolElement>("VoiceChat")
            .WithInteractability(false)
            .Value = info.LobbyInfo.VoiceChat;

        var slowMoElement = generalGroup.AddElement<EnumElement>("SlowMo")
            .WithInteractability(false);
        slowMoElement.EnumType = typeof(TimeScaleMode);
        slowMoElement.Value = info.LobbyInfo.SlowMoMode;

        // Player list
        element.PlayerBrowserElement.Clear();

        var playerListPage = element.PlayerBrowserElement.AddPage();

        foreach (var player in info.LobbyInfo.PlayerList.Players)
        {
            var playerResult = playerListPage.AddElement<PlayerResultElement>(player.Username);

            playerResult.GetReferences();

            playerResult.PlayerNameText.text = player.Username;

            playerResult.RoleText.text = player.PermissionLevel.ToString();

            playerResult.OnPressed = () =>
            {
                OnShowPlayer(player);
            };

            ElementIconHelper.SetProfileResultIcon(playerResult, player.AvatarTitle, player.AvatarModId);
        }

        // Disable unnecessary buttons
        element.CodeGrid.SetActive(false);
        element.BansGrid.SetActive(false);
    }

    private static void ApplyPlayerToElement(PlayerElement element, PlayerInfo info)
    {
        element.UsernameElement.Title = info.Username;

        element.NicknameElement.Title = "Nickname";
        element.NicknameElement.Value = info.Nickname;
        element.NicknameElement.EmptyFormat = "No {0}";

        element.DescriptionElement.Title = "Description";
        element.DescriptionElement.Value = string.Empty;
        element.DescriptionElement.EmptyFormat = "No {0}";

        element.PermissionsElement
            .WithTitle("Permissions")
            .WithColor(Color.yellow)
            .WithInteractability(false);

        element.PermissionsElement.Value = info.PermissionLevel;
        element.PermissionsElement.EnumType = typeof(PermissionLevel);

        ElementIconHelper.SetProfileIcon(element, info.AvatarTitle, info.AvatarModId);
    }
}
