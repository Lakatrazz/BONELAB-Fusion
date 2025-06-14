using Il2CppTMPro;

using LabFusion.Marrow.Proxies;
using LabFusion.Scene;
using LabFusion.SDK.Gamemodes;

using UnityEngine;

namespace LabFusion.Menu.Gamemodes;

public static class MenuGamemodeRounds
{
    public static MenuPage RoundsPage { get; private set; } = null;

    public static MenuPage RoundSettingsPage { get; private set; } = null;
    public static PageElement LevelRotationsElement { get; private set; } = null;
    public static FunctionElement NewLevelRotationElement { get; private set; } = null;
    public static TMP_Text SelectedLevelRotationText { get; private set; } = null;
    public static IntElement RotationRoundsElement { get; private set; } = null;
    public static BoolElement ShuffleLevelsElement { get; private set; } = null;
    public static IntElement TimeBetweenRoundsElement { get; private set; } = null;

    public static MenuPage LevelRotationSettingsPage { get; private set; } = null;
    public static StringElement LevelRotationNameElement { get; private set; } = null;
    public static FunctionElement DeleteRotationElement { get; private set; } = null;
    public static PageElement LevelsElement { get; private set; } = null;
    public static FunctionElement AddCurrentLevelElement { get; private set; } = null;

    public static void PopulateRounds(GameObject roundsPage)
    {
        RoundsPage = roundsPage.GetComponent<MenuPage>();

        // Round settings page
        var roundSettingsPage = roundsPage.transform.Find("page_RoundSettings");
        RoundSettingsPage = roundSettingsPage.GetComponent<MenuPage>();

        RoundSettingsPage.OnShown += OnRoundSettingsShown;

        var roundsLayout = roundSettingsPage.Find("grid_VerticalLayout");

        var levelRotationsGrid = roundsLayout.Find("grid_LevelRotations");
        LevelRotationsElement = levelRotationsGrid.Find("scrollRect_LevelRotations/Viewport/Content").GetComponent<PageElement>().AddPage();

        NewLevelRotationElement = levelRotationsGrid.Find("button_NewLevelRotation").GetComponent<FunctionElement>()
            .Do(CreateNewLevelRotation);

        SelectedLevelRotationText = roundsLayout.Find("text_SelectedRotation").GetComponent<TMP_Text>();

        RotationRoundsElement = roundsLayout.Find("button_RotationRounds").GetComponent<IntElement>()
            .WithTitle("Rounds Before Next Level")
            .WithLimits(1, 10)
            .WithValue(GamemodeRoundManager.Settings.RoundsBeforeNextLevel);
        RotationRoundsElement.OnValueChanged += OnRotationRoundsChanged;

        ShuffleLevelsElement = roundsLayout.Find("button_ShuffleLevels").GetComponent<BoolElement>()
            .WithTitle("Shuffle Levels")
            .WithValue(GamemodeRoundManager.Settings.ShuffleLevels);
        ShuffleLevelsElement.OnValueChanged += OnShuffleLevelsChanged;

        TimeBetweenRoundsElement = roundsLayout.Find("button_TimeBetweenRounds").GetComponent<IntElement>()
            .WithTitle("Time Between Rounds")
            .WithLimits(1, 60)
            .WithValue(GamemodeRoundManager.Settings.TimeBetweenRounds);
        TimeBetweenRoundsElement.OnValueChanged += OnTimeBetweenRoundsChanged;

        // Level rotation settings page
        var levelRotationSettingsPage = roundsPage.transform.Find("page_LevelRotationSettings");
        LevelRotationSettingsPage = levelRotationSettingsPage.GetComponent<MenuPage>();

        var levelRotationLayout = levelRotationSettingsPage.Find("grid_VerticalLayout");

        var optionsGrid = levelRotationLayout.Find("grid_LevelRotationOptions");
        LevelRotationNameElement = optionsGrid.Find("button_LevelRotationName").GetComponent<StringElement>()
            .WithTitle("Level Rotation Name");
        LevelRotationNameElement.OnValueChanged += OnLevelRotationNameChanged;
        LevelRotationNameElement.TextFormat = "{1}";
        LevelRotationNameElement.EmptyFormat = "Untitled";

        DeleteRotationElement = optionsGrid.Find("button_DeleteRotation").GetComponent<FunctionElement>()
            .Do(DeleteSelectedLevelRotation);

        LevelsElement = levelRotationLayout.Find("scrollRect_Levels/Viewport/Content").GetComponent<PageElement>().AddPage();

        AddCurrentLevelElement = levelRotationLayout.Find("button_AddCurrentLevel").GetComponent<FunctionElement>()
            .WithTitle("Add Current Level")
            .Do(AddCurrentLevel);
    }

    private static string _shownGamemode = null;
    private static LevelRotation _shownRotation = null;

    public static void ShowLevelRotations(string gamemodeBarcode)
    {
        _shownGamemode = gamemodeBarcode;

        RoundsPage.SelectSubPage(RoundSettingsPage);
    }

    private static void OnRoundSettingsShown()
    {
        if (string.IsNullOrWhiteSpace(_shownGamemode))
        {
            return;
        }

        LoadLevelRotations();
    }

    public static void ShowLevelRotation(LevelRotation rotation)
    {
        _shownRotation = rotation;

        RoundsPage.SelectSubPage(LevelRotationSettingsPage);

        LevelRotationNameElement.Value = rotation.Name;

        LoadLevels();
    }

    private static void LoadLevels()
    {
        LevelsElement.Clear();

        if (_shownRotation == null)
        {
            return;
        }

        foreach (var barcode in _shownRotation.LevelBarcodes)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                continue;
            }

            var element = LevelsElement.AddElement<LevelResultElement>(barcode);

            element.ApplyLevel(barcode);

            element.OnRemovePressed += () =>
            {
                _shownRotation.LevelBarcodes.Remove(barcode);

                LoadLevels();
            };
        }
    }

    private static void LoadLevelRotations()
    {
        LevelRotationsElement.Clear();

        if (string.IsNullOrWhiteSpace(_shownGamemode))
        {
            return;
        }

        var rotations = GamemodeRoundManager.Settings.GetRotationsByGamemode(_shownGamemode);

        List<LevelRotationResultElement> elements = new();

        foreach (var rotation in rotations.LevelRotations)
        {
            var element = LevelRotationsElement.AddElement<LevelRotationResultElement>(rotation.Name);

            elements.Add(element);

            element.LevelRotationNameText.text = string.IsNullOrWhiteSpace(rotation.Name) ? "Untitled" : rotation.Name;

            string levelBarcode = null;

            if (rotation.LevelBarcodes.Count > 0)
            {
                levelBarcode = rotation.LevelBarcodes[0];
            }

            element.ApplyLevelIcon(levelBarcode);

            element.Highlight(GamemodeRoundManager.ActiveRotation == rotation);

            element.OnSelectPressed += () =>
            {
                UnhiglightElements();

                if (GamemodeRoundManager.ActiveRotation != rotation)
                {
                    GamemodeRoundManager.ActiveRotation = rotation;

                    element.Highlight(true);
                }
                else
                {
                    GamemodeRoundManager.ActiveRotation = null;
                }

                UpdateSelectedText();
            };

            element.OnEditPressed += () =>
            {
                ShowLevelRotation(rotation);
            };
        }

        UpdateSelectedText();

        void UnhiglightElements()
        {
            foreach (var element in elements)
            {
                element.Highlight(false);
            }
        }
    }

    private static void UpdateSelectedText()
    {
        var rotation = GamemodeRoundManager.ActiveRotation;

        if (rotation == null)
        {
            SelectedLevelRotationText.text = "No Rotation Selected";
        }
        else
        {
            string name = string.IsNullOrWhiteSpace(rotation.Name) ? "Untitled" : rotation.Name;
            SelectedLevelRotationText.text = $"{name} Selected";
        }
    }

    private static void OnRotationRoundsChanged(int value)
    {
        GamemodeRoundManager.Settings.RoundsBeforeNextLevel = value;
        GamemodeRoundManager.SaveSettings();
    }

    private static void OnShuffleLevelsChanged(bool value)
    {
        GamemodeRoundManager.Settings.ShuffleLevels = value;
        GamemodeRoundManager.SaveSettings();
    }

    private static void OnTimeBetweenRoundsChanged(int value)
    {
        GamemodeRoundManager.Settings.TimeBetweenRounds = value;
        GamemodeRoundManager.SaveSettings();
    }

    private static void OnLevelRotationNameChanged(string value)
    {
        if (_shownRotation == null)
        {
            return;
        }

        _shownRotation.Name = value;
        GamemodeRoundManager.SaveSettings();
    }

    private static void CreateNewLevelRotation()
    {
        if (string.IsNullOrWhiteSpace(_shownGamemode))
        {
            return;
        }

        var rotations = GamemodeRoundManager.Settings.GetRotationsByGamemode(_shownGamemode);

        var newRotation = new LevelRotation();

        rotations.LevelRotations.Add(newRotation);

        GamemodeRoundManager.SaveSettings();

        ShowLevelRotation(newRotation);
    }

    private static void DeleteSelectedLevelRotation()
    {
        if (_shownRotation == null || string.IsNullOrWhiteSpace(_shownGamemode))
        {
            return;
        }

        if (GamemodeRoundManager.ActiveRotation == _shownRotation)
        {
            GamemodeRoundManager.ActiveRotation = null;
        }

        var rotations = GamemodeRoundManager.Settings.GetRotationsByGamemode(_shownGamemode);

        rotations.LevelRotations.Remove(_shownRotation);

        _shownRotation = null;

        GamemodeRoundManager.SaveSettings();

        ShowLevelRotations(_shownGamemode);
    }

    private static void AddCurrentLevel()
    {
        if (_shownRotation == null)
        {
            return;
        }

        var levelBarcode = FusionSceneManager.Barcode;

        if (string.IsNullOrWhiteSpace(levelBarcode) || _shownRotation.LevelBarcodes.Contains(levelBarcode))
        {
            return;
        }

        _shownRotation.LevelBarcodes.Add(levelBarcode);

        LoadLevels();
    }
}