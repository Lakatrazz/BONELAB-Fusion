﻿using LabFusion.Marrow.Proxies;
using LabFusion.Menu.Gamemodes;
using LabFusion.Network;
using LabFusion.Scene;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;
using UnityEngine;

namespace LabFusion.Menu;

public static class MenuMatchmakingGamemodes
{
    public static MenuPage GamemodesMenuPage { get; private set; }

    public static PageElement GamemodesPageElement { get; private set; }

    public static FunctionElement FindServerElement { get; private set; }
    public static FunctionElement CreateServerElement { get; private set; }

    public static LabelElement SearchingLabel { get; private set; }

    public static LabelElement NoneFoundLabel { get; private set; }
    public static FunctionElement CreateNewElement { get; private set; }

    public static Gamemode SelectedGamemode { get; private set; }

    private static void OnSearchFailure()
    {
        GamemodesMenuPage.Parent.SelectSubPage(GamemodesMenuPage);

        GamemodesMenuPage.SelectSubPage(2);

        string noneFoundText = "No Gamemode servers found!\nCreate a new one?";

        if (SelectedGamemode != null)
        {
            noneFoundText = $"No {SelectedGamemode.Title} servers found!\nCreate a new one?";
        }

        NoneFoundLabel.Title = noneFoundText;
    }

    private static void FindGamemodeServer()
    {
        GamemodesMenuPage.SelectSubPage(1);

        string searchingText = "Searching for active Gamemode servers...";

        if (SelectedGamemode != null)
        {
            searchingText = $"Searching for active {SelectedGamemode.Title} servers...";
        }

        SearchingLabel.Title = searchingText;

       var matchmaker = NetworkInfo.CurrentNetworkLayer.Matchmaker;

        if (matchmaker != null)
        {
            matchmaker.RequestLobbies(OnLobbiesRequested);
        }
        else
        {
            OnSearchFailure();
        }
    }

    private static void OnLobbiesRequested(IMatchmaker.MatchmakerCallbackInfo info)
    {
        var gamemodeLobbies = info.lobbies
            .Where((lobby) => lobby.metadata.LobbyInfo.GamemodeBarcode != string.Empty);

        bool foundLobbies = MenuMatchmaking.LoadLobbiesIntoBrowser(gamemodeLobbies);

        if (!foundLobbies)
        {
            OnSearchFailure();
        }
    }

    private static void CreateGamemodeServer()
    {
        if (SelectedGamemode == null)
        {
            return;
        }

        GamemodeHelper.StartGamemodeServer(SelectedGamemode);
    }

    public static void PopulateGamemodes(Transform gamemodesTransform)
    {
        GamemodesMenuPage = gamemodesTransform.GetComponent<MenuPage>();

        // Query page
        var queryPage = gamemodesTransform.Find("page_Query");

        var queryLayout = queryPage.Find("layout_Options");

        GamemodesPageElement = queryLayout.Find("scrollRect_Gamemodes/Viewport/Content").GetComponent<PageElement>().AddPage();

        var actionsLayout = queryLayout.Find("layout_Actions");

        FindServerElement = actionsLayout.Find("button_FindServer").GetComponent<FunctionElement>()
            .WithTitle("Find Server")
            .Do(FindGamemodeServer);

        CreateServerElement = actionsLayout.Find("button_CreateServer").GetComponent<FunctionElement>()
            .WithTitle("Create Server")
            .Do(CreateGamemodeServer);

        // Searching page
        var searchingPage = gamemodesTransform.Find("page_Searching");

        SearchingLabel = searchingPage.Find("label_Searching").GetComponent<LabelElement>();
        SearchingLabel.Title = "Searching for active Gamemodes...";

        // Create new page
        var createNewPage = gamemodesTransform.Find("page_CreateNew");

        var createNewLayout = createNewPage.Find("layout_Options");

        NoneFoundLabel = createNewLayout.Find("label_NoneFound").GetComponent<LabelElement>();

        CreateNewElement = createNewLayout.Find("button_CreateNew").GetComponent<FunctionElement>()
            .WithTitle("Create New")
            .Do(CreateGamemodeServer);

        // Refresh the list of gamemodes
        RefreshGamemodes();
    }

    private static void UpdateButtonVisibilities()
    {
        bool visible = SelectedGamemode != null;
        FindServerElement.gameObject.SetActive(visible);
        CreateServerElement.gameObject.SetActive(visible);
    }

    public static void RefreshGamemodes()
    {
        GamemodeListHelper.RefreshGamemodeList(GamemodesPageElement, (gamemode) =>
        {
            SelectedGamemode = gamemode;

            UpdateButtonVisibilities();
        });
    }
}