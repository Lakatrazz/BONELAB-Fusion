using LabFusion.Extensions;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuMatchmakingSandbox
{
    public static MenuPage SandboxMenuPage { get; private set; }

    public static FunctionElement QuickPlayElement { get; private set; }

    public static LabelElement SearchingLabel { get; private set; }

    public static LabelElement NoneFoundLabel { get; private set; }
    public static FunctionElement CreateNewElement { get; private set; }

    private static void OnSearchFailure()
    {
        SandboxMenuPage.Parent.SelectSubPage(SandboxMenuPage);

        SandboxMenuPage.SelectSubPage(2);

        string noneFoundText = "No Sandbox servers found!\nCreate a new one?";

        NoneFoundLabel.Title = noneFoundText;
    }

    private static void FindSandboxServer()
    {
        SandboxMenuPage.SelectSubPage(1);

        string searchingText = "Searching for Sandbox servers...";

        SearchingLabel.Title = searchingText;

        var matchmaker = NetworkLayerManager.Layer.Matchmaker;

        if (matchmaker != null)
        {
            matchmaker.RequestLobbies(OnLobbiesRequested);
        }
        else
        {
            OnSearchFailure();
        }
    }

    private static bool CheckLobbyValidation(IMatchmaker.LobbyInfo lobby)
    {
        var lobbyInfo = lobby.Metadata.LobbyInfo;

        // Make sure this lobby is on the right Fusion version
        if (NetworkVerification.CompareVersion(lobbyInfo.LobbyVersion, FusionMod.Version) != VersionResult.Ok)
        {
            return false;
        }

        // Check if the lobby is full
        if (lobbyInfo.PlayerCount >= lobbyInfo.MaxPlayers)
        {
            return false;
        }
        
        // Check if we have the lobby's level
        if (!lobby.Metadata.ClientHasLevel)
        {
            return false;
        }

        // This lobby has a Gamemode, but we are only looking for Sandbox lobbies
        if (!string.IsNullOrWhiteSpace(lobbyInfo.GamemodeBarcode))
        {
            return false;
        }

        return true;
    }

    private static void OnLobbiesRequested(IMatchmaker.MatchmakerCallbackInfo info)
    {
        var sandboxLobbies = info.Lobbies
            .Where(CheckLobbyValidation);

        var validLobbies = MenuMatchmaking.ValidateLobbies(sandboxLobbies);

        // No valid lobbies?
        if (!validLobbies.Any())
        {
            OnSearchFailure();
            return;
        }

        // Otherwise, join a random one
        var randomLobby = validLobbies.GetRandom();

        randomLobby.Metadata.CreateJoinDelegate(randomLobby.Lobby)?.Invoke();

    }

    private static void CreateSandboxServer()
    {
        if (NetworkInfo.HasServer)
        {
            NetworkHelper.Disconnect();
        }

        NetworkHelper.StartServer();
    }

    public static void PopulateSandbox(Transform sandboxTransform)
    {
        SandboxMenuPage = sandboxTransform.GetComponent<MenuPage>();

        // Quick Play page
        var quickPlayPage = sandboxTransform.Find("page_QuickPlay");

        QuickPlayElement = quickPlayPage.Find("button_QuickPlay").GetComponent<FunctionElement>().
            WithTitle("Quick Play")
            .Do(FindSandboxServer);

        // Searching page
        var searchingPage = sandboxTransform.Find("page_Searching");

        SearchingLabel = searchingPage.Find("label_Searching").GetComponent<LabelElement>();
        SearchingLabel.Title = "Searching for Sandbox servers...";

        // Create new page
        var createNewPage = sandboxTransform.Find("page_CreateNew");

        var createNewLayout = createNewPage.Find("layout_Options");

        NoneFoundLabel = createNewLayout.Find("label_NoneFound").GetComponent<LabelElement>();

        CreateNewElement = createNewLayout.Find("button_CreateNew").GetComponent<FunctionElement>()
            .WithTitle("Create New")
            .Do(CreateSandboxServer);
    }
}
