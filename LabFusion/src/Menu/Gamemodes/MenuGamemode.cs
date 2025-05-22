using LabFusion.Marrow.Proxies;
using LabFusion.Menu.Data;
using LabFusion.Network;
using LabFusion.SDK.Gamemodes;

using UnityEngine;
using UnityEngine.UI;
using static Il2CppSLZ.Bonelab.BaseGameController;

namespace LabFusion.Menu.Gamemodes;

public static class MenuGamemode
{
    // Options grid
    public static RawImage GamemodeIcon { get; private set; } = null;

    public static LabelElement GamemodeTitle { get; private set; } = null;

    public static LabelElement GamemodeReadyElement { get; private set; } = null;

    public static LabelElement GamemodeStartedElement { get; private set; } = null;

    // Selection grid
    public static GameObject GamemodeSelectionGrid { get; private set; } = null;

    public static GameObject SettingsGrid { get; private set; } = null;

    public static PageElement SettingsPageElement { get; private set; } = null;

    public static PageElement GamemodesPageElement { get; private set; } = null;

    public static FunctionElement SelectGamemodeElement { get; private set; } = null;

    public static FunctionElement ExitGamemodeElement { get; private set; } = null;

    public static Gamemode SelectedGamemode { get; private set; } = null;

    public static void OnInitializeMelon()
    {
        GamemodeManager.OnGamemodeChanged += OnGamemodeChanged;
        GamemodeManager.OnGamemodeStarted += OnGamemodeStarted;
        GamemodeManager.OnGamemodeStopped += OnGamemodeStopped;
        GamemodeManager.OnGamemodeReady += OnGamemodeReady;
        GamemodeManager.OnGamemodeUnready += OnGamemodeUnready;
        GamemodeManager.OnStartTimerChanged += OnStartTimerChanged;
    }

    private static void OnGamemodeChanged(Gamemode gamemode)
    {
        UpdateMenuGamemode();
    }

    private static void OnGamemodeStarted()
    {
        UpdateMenuGamemode();
    }

    private static void OnGamemodeStopped()
    {
        UpdateMenuGamemode();
    }

    private static void OnGamemodeReady()
    {
        UpdateReadyLabel();
    }

    private static void OnGamemodeUnready()
    {
        UpdateReadyLabel();
    }

    private static void OnStartTimerChanged(float value)
    {
        UpdateStartedLabel();
    }

    private static void UpdateStartedLabel()
    {
        if (GamemodeManager.StartTimerActive)
        {
            GamemodeStartedElement.Title = $"Starting in {Mathf.CeilToInt(GamemodeManager.StartTimer)} seconds...";
        }
        else
        {
            GamemodeStartedElement.Title = $"{(GamemodeManager.IsGamemodeStarted ? "Started" : "Not Started")}";
        }
    }

    private static void UpdateReadyLabel()
    {
        GamemodeReadyElement.Title = $"{(GamemodeManager.IsGamemodeReady ? "Ready" : "Not Ready")}";
    }

    private static void OverrideSettingsPage(Gamemode gamemode)
    {
        if (GamemodeManager.ActiveGamemode != null)
        {
            return;
        }

        SettingsPageElement.Clear();

        if (gamemode == null)
        {
            SettingsGrid.SetActive(false);
            return;
        }

        SettingsGrid.SetActive(true);

        if (NetworkInfo.IsHost)
        {
            ApplySettingsData(gamemode);
        }
    }

    private static void ApplySettingsData(Gamemode gamemode)
    {
        var settingsGroup = gamemode.CreateSettingsGroup();

        if (settingsGroup.Elements.Count > 0)
        {
            ElementDataHelper.ApplyGroupData(SettingsPageElement, settingsGroup);
        }
        else
        {
            SettingsGrid.SetActive(false);
        }
    }

    private static void UpdateMenuGamemode()
    {
        if (GamemodeIcon == null)
        {
            return;
        }

        SettingsPageElement.Clear();
        SettingsGrid.SetActive(true);

        GamemodeSelectionGrid.gameObject.SetActive(NetworkInfo.IsHost);

        var activeGamemode = GamemodeManager.ActiveGamemode;

        if (activeGamemode != null)
        {
            var logo = activeGamemode.Logo ? activeGamemode.Logo : MenuResources.GetGamemodeIcon(MenuResources.ModsIconTitle);

            GamemodeIcon.texture = logo;

            GamemodeTitle.Title = activeGamemode.Title;

            GamemodeSelectionGrid.gameObject.SetActive(NetworkInfo.IsHost);

            ApplySettingsData(activeGamemode);

            GamemodeReadyElement.gameObject.SetActive(true);
            GamemodeStartedElement.gameObject.SetActive(true);

            UpdateReadyLabel();
            UpdateStartedLabel();
        }
        else
        {
            GamemodeIcon.texture = MenuResources.GetGamemodeIcon(MenuResources.SandboxIconTitle);

            GamemodeTitle.Title = "Sandbox";

            GamemodeReadyElement.gameObject.SetActive(false);
            GamemodeStartedElement.gameObject.SetActive(false);

            RefreshGamemodes();
        }

        UpdateActionElements();
    }

    private static void OnSelectGamemodePressed()
    {
        if (SelectedGamemode == null)
        {
            return;
        }

        GamemodeManager.SelectGamemode(SelectedGamemode);
    }

    private static void OnExitGamemodePressed()
    {
        GamemodeManager.DeselectGamemode();
    }

    public static void PopulateGamemode(GameObject gamemodePage)
    {
        // Options grid
        var optionsGrid = gamemodePage.transform.Find("grid_GamemodeOptions");

        GamemodeIcon = optionsGrid.Find("label_GamemodeIcon/icon_Mask/icon_Gamemode").GetComponent<RawImage>();

        GamemodeTitle = optionsGrid.Find("label_GamemodeTitle").GetComponent<LabelElement>();

        GamemodeReadyElement = optionsGrid.Find("label_GamemodeReady").GetComponent<LabelElement>();

        GamemodeStartedElement = optionsGrid.Find("label_GamemodeStarted").GetComponent<LabelElement>();

        // Selection grid
        var selectionGrid = gamemodePage.transform.Find("grid_GamemodeSelection");

        GamemodeSelectionGrid = selectionGrid.gameObject;

        SettingsGrid = selectionGrid.Find("grid_Settings").gameObject;
        SettingsPageElement = SettingsGrid.transform.Find("scrollRect_Settings/Viewport/Content").GetComponent<PageElement>().AddPage();

        GamemodesPageElement = selectionGrid.Find("scrollRect_Gamemodes/Viewport/Content").GetComponent<PageElement>().AddPage();

        SelectGamemodeElement = selectionGrid.Find("button_SelectGamemode").GetComponent<FunctionElement>()
            .WithTitle("Select Gamemode")
            .Do(OnSelectGamemodePressed);

        ExitGamemodeElement = selectionGrid.Find("button_ExitGamemode").GetComponent<FunctionElement>()
            .WithTitle("Exit Gamemode")
            .Do(OnExitGamemodePressed);

        UpdateMenuGamemode();

        RefreshGamemodes();
    }

    private static void UpdateActionElements()
    {
        bool newGamemodeSelected = SelectedGamemode != null && SelectedGamemode != GamemodeManager.ActiveGamemode;

        SelectGamemodeElement.gameObject.SetActive(NetworkInfo.IsHost && newGamemodeSelected);

        if (newGamemodeSelected)
        {
            SelectGamemodeElement.Title = $"Select {SelectedGamemode.Title}";
        }

        ExitGamemodeElement.gameObject.SetActive(NetworkInfo.IsHost && GamemodeManager.ActiveGamemode != null);

        if (GamemodeManager.ActiveGamemode != null)
        {
            ExitGamemodeElement.Title = $"Exit {GamemodeManager.ActiveGamemode.Title}";
        }
    }

    public static void RefreshGamemodes()
    {
        GamemodeListHelper.RefreshGamemodeList(GamemodesPageElement, (gamemode) =>
        {
            SelectedGamemode = gamemode;

            OverrideSettingsPage(gamemode);

            UpdateActionElements();
        });
    }
}