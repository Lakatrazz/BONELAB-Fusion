using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Safety;
using LabFusion.SDK.Lobbies;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuMatchmaking
{
    public static MenuPage MatchmakingPage { get; private set; }

    // Filters
    public static GameObject BrowserFiltersRoot { get; private set; }
    public static PageElement BrowserFiltersElement { get; private set; }

    public static PageElement SandboxFiltersPageElement { get; private set; }

    public static GroupElement SandboxFiltersGroupElement { get; private set; }

    // Page Buttons
    public static FunctionElement GamemodeOptionElement { get; private set; }
    public static FunctionElement SandboxOptionElement { get; private set; }
    public static FunctionElement BrowseOptionElement { get; private set; }
    public static FunctionElement CodeOptionElement { get; private set; }

    // Code
    public static StringElement CodeElement { get; private set; }

    // Browser
    public static MenuPage BrowserPage { get; private set; }

    public static PageElement LobbyBrowserElement { get; private set; }
    public static PageElement SearchResultsElement { get; private set; }

    public static StringElement SearchBarElement { get; private set; }
    public static FunctionElement RefreshElement { get; private set; }

    public static LabelElement NoServersFoundElement { get; private set; }

    // Lobby
    public static LobbyElement LobbyPanel { get; private set; }

    public static void PopulateMatchmaking(GameObject matchmakingPage)
    {
        MatchmakingPage = matchmakingPage.GetComponent<MenuPage>();

        MatchmakingPage.OnShown += OnMatchmakingPageShown;

        // Get options references
        var optionsTransform = matchmakingPage.transform.Find("page_Options");

        PopulateOptions(optionsTransform);

        // Get sandbox references
        var sandboxTransform = matchmakingPage.transform.Find("page_Sandbox");

        MenuMatchmakingSandbox.PopulateSandbox(sandboxTransform);

        // Get gamemodes references
        var gamemodesTransform = matchmakingPage.transform.Find("page_Gamemodes");

        MenuMatchmakingGamemodes.PopulateGamemodes(gamemodesTransform);

        // Get code references
        var codeTransform = matchmakingPage.transform.Find("page_Code");

        PopulateCode(codeTransform);

        // Get browser references
        var browserTransform = matchmakingPage.transform.Find("page_Browser");

        PopulateBrowser(browserTransform);
    }
    
    private static void OnMatchmakingPageShown()
    {
        var networkLayer = NetworkLayerManager.Layer;

        bool supportsMatchmaking = networkLayer.Matchmaker != null;
        
        GamemodeOptionElement.gameObject.SetActive(supportsMatchmaking);
        SandboxOptionElement.gameObject.SetActive(supportsMatchmaking);
        BrowseOptionElement.gameObject.SetActive(supportsMatchmaking);
    }

    private static void PopulateOptions(Transform optionsTransform)
    {
        var grid = optionsTransform.Find("grid_Options");

        // Gamemode
        GamemodeOptionElement = grid.Find("button_Gamemode").GetComponent<FunctionElement>();

        GamemodeOptionElement.transform.Find("label_Title").GetComponent<LabelElement>().Title = "Gamemode";
        GamemodeOptionElement.Do(() =>
        {
            MatchmakingPage.SelectSubPage(1);
        });

        // Sandbox
        SandboxOptionElement = grid.Find("button_Sandbox").GetComponent<FunctionElement>();
        SandboxOptionElement.Do(() =>
        {
            MatchmakingPage.SelectSubPage(2);

            RefreshBrowser();
        });

        SandboxOptionElement.transform.Find("label_Title").GetComponent<LabelElement>().Title = "Sandbox";

        // Browse
        BrowseOptionElement = grid.Find("button_Browse").GetComponent<FunctionElement>();
        BrowseOptionElement.Do(() =>
        {
            MatchmakingPage.SelectSubPage(4);

            RefreshBrowser();
        });

        BrowseOptionElement.transform.Find("label_Title").GetComponent<LabelElement>().Title = "Browse";

        // Enter Code
        CodeOptionElement = grid.Find("button_Code").GetComponent<FunctionElement>();
        CodeOptionElement.Do(() =>
        {
            MatchmakingPage.SelectSubPage(3);
        });

        CodeOptionElement.transform.Find("label_Title").GetComponent<LabelElement>().Title = "Enter Code";
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

        BrowserFiltersRoot = filtersGroup.gameObject;
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

        RefreshElement = searchGroup.Find("button_Refresh").GetComponent<FunctionElement>();
        RefreshElement.Do(RefreshBrowser);

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

        NetworkLayerManager.Layer.Matchmaker?.RequestLobbies(OnLobbiesRequested);
    }

    private static bool CheckLobbyVisibility(IMatchmaker.LobbyInfo info)
    {
        switch (info.Metadata.LobbyInfo.Privacy)
        {
            case ServerPrivacy.PUBLIC:
                return true;
            case ServerPrivacy.FRIENDS_ONLY:
                return NetworkLayerManager.Layer.IsFriend(info.Metadata.LobbyInfo.LobbyId);
            default:
                return false;
        }
    }

    private static bool CheckLobbySearch(IMatchmaker.LobbyInfo info, string query)
    {
        var metadata = info.Metadata;
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

    public static IEnumerable<IMatchmaker.LobbyInfo> ValidateLobbies(IEnumerable<IMatchmaker.LobbyInfo> lobbies)
    {
        return lobbies.Where(CheckLobbyVisibility);
    }

    private static IEnumerable<IMatchmaker.LobbyInfo> SortLobbies(IEnumerable<IMatchmaker.LobbyInfo> lobbies)
    {
        return lobbies
            .OrderBy(l => l.Metadata.LobbyInfo.LobbyHostName)
            .OrderByDescending(l => l.Metadata.LobbyInfo.PlayerCount)
            .OrderByDescending(l => l.Metadata.LobbyInfo.LobbyVersion)
            .OrderBy(l => l.Metadata.LobbyInfo.LevelTitle)
            .Where(CheckLobbyVisibility);
    }

    public static bool LoadLobbiesIntoBrowser(IEnumerable<IMatchmaker.LobbyInfo> lobbies) 
    {
        MatchmakingPage.SelectSubPage(4);

        // Clear lobbies
        SearchResultsElement.RemoveElements<LobbyResultElement>();

        // Sort lobbies
        var sortedLobbies = SortLobbies(lobbies);

        // Add all lobbies to the list
        foreach (var lobby in sortedLobbies)
        {
            var lobbyResult = SearchResultsElement.AddElement<LobbyResultElement>("Lobby Result");

            ApplyLobbyToResult(lobbyResult, lobby);
        }

        bool foundLobbies = sortedLobbies.Any();
        NoServersFoundElement.gameObject.SetActive(!foundLobbies);

        return foundLobbies;
    }

    private static void OnLobbiesRequested(IMatchmaker.MatchmakerCallbackInfo info)
    {
        _isSearchingLobbies = false;

        var sortedLobbies = SortLobbies(info.Lobbies)
            .Where(l => LobbyFilterManager.CheckOptionalFilters(l.Lobby, l.Metadata))
            .Where(l => LobbyFilterManager.CheckPersistentFilters(l.Lobby, l.Metadata));

        // Enable buttons
        SearchBarElement.gameObject.SetActive(true);
        RefreshElement.gameObject.SetActive(true);
        BrowserFiltersRoot.SetActive(true);

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

        ApplyServerMetadataToLobby(LobbyPanel, info.Lobby, info.Metadata);
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

        var metadata = info.Metadata;

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

        element.ServerNameText.text = ProfanityFilter.Filter(ParseServerName(metadata.LobbyInfo.LobbyName, metadata.LobbyInfo.LobbyHostName)).RemoveRichTextExceptColor();
        element.HostNameText.text = ProfanityFilter.Filter(metadata.LobbyInfo.LobbyHostName).RemoveRichTextExceptColor();

        element.PlayerCountText.text = string.Format($"{metadata.LobbyInfo.PlayerCount}/{metadata.LobbyInfo.MaxPlayers} Players");
        element.PlayerCountText.color = playerCountColor;

        element.VersionText.text = string.Format($"v{metadata.LobbyInfo.LobbyVersion}");
        element.VersionText.color = versionColor;

        ElementIconHelper.SetLevelResultIcon(element, metadata.LobbyInfo.LevelTitle, metadata.LobbyInfo.LevelModId);

        // Gamemode icon
        var gamemodeIcon = MenuResources.GetGamemodeIcon(MenuResources.SandboxIconTitle);

        if (!string.IsNullOrWhiteSpace(metadata.LobbyInfo.GamemodeTitle))
        {
            gamemodeIcon = MenuResources.GetGamemodeIcon(metadata.LobbyInfo.GamemodeTitle);
        }

        if (gamemodeIcon == null)
        {
            gamemodeIcon = MenuResources.GetGamemodeIcon(MenuResources.ModsIconTitle);
        }

        element.GamemodeIcon.texture = gamemodeIcon;
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

        element.ServerNameElement.Value = ProfanityFilter.Filter(ParseServerName(info.LobbyInfo.LobbyName, info.LobbyInfo.LobbyHostName)).RemoveRichTextExceptColor();

        element.HostNameElement
            .WithTitle(ProfanityFilter.Filter(info.LobbyInfo.LobbyHostName));

        element.DescriptionElement
            .Cleared()
            .WithTitle("Description");

        element.DescriptionElement.EmptyFormat = "No {0}";
        element.DescriptionElement.TextFormat = "{1}";

        element.DescriptionElement.Value = ProfanityFilter.Filter(info.LobbyInfo.LobbyDescription).RemoveRichTextExceptColor();

        element.MoreElement
            .Cleared()
            .WithTitle("More...")
            .Do(() => { element.LobbyPage.SelectSubPage(1); });

        ElementIconHelper.SetLevelIcon(element, info.LobbyInfo.LevelTitle, info.LobbyInfo.LevelModId);
        ElementIconHelper.SetGamemodeIcon(element, info.LobbyInfo.GamemodeTitle);

        // Fill out lists
        // Settings list
        element.SettingsElement.Clear();

        var settingsPage = element.SettingsElement.AddPage();

        var generalGroup = settingsPage.AddElement<GroupElement>("General");

        generalGroup.AddElement<BoolElement>("NameTags")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.NameTags);

        generalGroup.AddElement<BoolElement>("VoiceChat")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.VoiceChat);

        generalGroup.AddElement<EnumElement>("SlowMo")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.SlowMoMode);

        generalGroup.AddElement<BoolElement>("Mortality")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.Mortality);

        generalGroup.AddElement<BoolElement>("Friendly Fire")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.FriendlyFire);

        generalGroup.AddElement<BoolElement>("Knockout")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.Knockout);

        generalGroup.AddElement<IntElement>("Knockout Length")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.KnockoutLength);

        generalGroup.AddElement<BoolElement>("Player Constraining")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.PlayerConstraining);

        generalGroup.AddElement<FloatElement>("Max Avatar Height")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.MaxAvatarHeight);

        // Permissions
        var permissionsGroup = settingsPage.AddElement<GroupElement>("Permissions");

        permissionsGroup.AddElement<EnumElement>("Dev Tools")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.DevTools);

        permissionsGroup.AddElement<EnumElement>("Constrainer")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.Constrainer);

        permissionsGroup.AddElement<EnumElement>("Custom Avatars")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.CustomAvatars);

        permissionsGroup.AddElement<EnumElement>("Kicking")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.Kicking);

        permissionsGroup.AddElement<EnumElement>("Banning")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.Banning);

        permissionsGroup.AddElement<EnumElement>("Teleportation")
            .WithInteractability(false)
            .WithValue(info.LobbyInfo.Teleportation);

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
        element.UsernameElement.Title = TextFilter.Filter(info.Username);

        element.NicknameElement.Title = "Nickname";
        element.NicknameElement.Value = TextFilter.Filter(info.Nickname);
        element.NicknameElement.EmptyFormat = "No {0}";

        element.DescriptionElement.Title = "Description";
        element.DescriptionElement.Value = TextFilter.Filter(info.Description);
        element.DescriptionElement.EmptyFormat = "No {0}";

        element.PermissionsElement
            .WithTitle("Permissions")
            .WithColor(Color.yellow)
            .WithInteractability(false);

        element.PermissionsElement.Value = info.PermissionLevel;
        element.PermissionsElement.EnumType = typeof(PermissionLevel);

        element.PlatformIDElement
            .WithTitle("Platform ID")
            .WithColor(Color.red)
            .WithInteractability(false);

        element.PlatformIDElement.Value = info.LongId.ToString();

        ElementIconHelper.SetProfileIcon(element, info.AvatarTitle, info.AvatarModId);

        // Disable unnecessary elements
        element.VolumeElement.gameObject.SetActive(false);
    }
}
