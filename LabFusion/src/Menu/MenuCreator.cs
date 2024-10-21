using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;

using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;
using LabFusion.Utilities;

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Menu;

public static class MenuCreator
{
    public static int MenuPageIndex => _menuPageIndex;
    private static int _menuPageIndex = -1;

    public static Image MenuButtonImage => _menuButtonImage;
    private static Image _menuButtonImage = null;

    public static MenuPopups MenuPopups => _menuPopups;
    private static MenuPopups _menuPopups = null;

    public static void OnInitializeMelon()
    {
        MenuPageHelper.OnInitializeMelon();
    }

    public static void CreateMenu()
    {
        CreateMenuButton();

        SpawnMenu();
    }

    private static void CreateMenuButton()
    {
        // Get references to the UI Rig
        var uiRig = UIRig.Instance;

        if (uiRig == null)
        {
            return;
        }

        // Add the button
        var panelView = uiRig.popUpMenu.preferencesPanelView;

        var gridOptions = panelView.transform.Find("page_OPTIONS/grid_Options");
        var controlButton = gridOptions.Find("button_Control").gameObject;

        var fusionButton = GameObject.Instantiate(controlButton, controlButton.transform.parent, false);
        fusionButton.name = "button_Fusion";

        var quitButtonIndex = gridOptions.Find("button_Quit").GetSiblingIndex();

        fusionButton.transform.SetSiblingIndex(quitButtonIndex - 1);

        // Modify the button
        var buttonText = fusionButton.transform.Find("text_Control").GetComponent<TMP_Text>();
        buttonText.text = "Fusion";

        buttonText.gameObject.name = "text_Fusion";

        var buttonScript = fusionButton.GetComponent<Button>();
        buttonScript.onClick.RemoveAllListeners();
        buttonScript.AddClickEvent(OnMenuButtonClicked);

        // Cache references
        _menuButtonImage = fusionButton.transform.Find("img_icon").GetComponent<Image>();
    }

    private static void OnMenuButtonClicked()
    {
        // Make sure the page has been spawned properly
        if (_menuPageIndex < 0)
        {
            return;
        }

        // Get references to the UI Rig
        var uiRig = UIRig.Instance;

        if (uiRig == null)
        {
            return;
        }

        // Open the page
        var panelView = uiRig.popUpMenu.preferencesPanelView;

        panelView.PAGESELECT(_menuPageIndex);
    }

    private static void SpawnMenu()
    {
        // Register and spawn the menu spawnable
        var spawnable = new Spawnable()
        {
            crateRef = FusionSpawnableReferences.FusionMenuReference,
            policyData = null,
        };

        AssetSpawner.Register(spawnable);

        SafeAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, OnMenuSpawned);
    }

    private static void OnMenuSpawned(Poolee poolee)
    {
        // Get references to the UI Rig
        var uiRig = UIRig.Instance;

        if (uiRig == null)
        {
            return;
        }

        var panelView = uiRig.popUpMenu.preferencesPanelView;

        // Inject into the preferences menu
        var menuGameObject = poolee.gameObject;
        var menuTransform = poolee.transform;

        menuTransform.parent = panelView.transform;
        menuTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        menuTransform.localScale = Vector3.one;

        _menuPageIndex = InjectPage(panelView, menuGameObject);

        // Cache references
        _menuPopups = menuGameObject.GetComponentInChildren<MenuPopups>(true);

        // Setup text
        MenuButtonHelper.PopulateTexts(menuGameObject);

        // Setup buttons
        MenuButtonHelper.PopulateButtons(menuGameObject);

        // Get resources contained in the menu
        MenuResources.GetResources(menuTransform.Find("Resources"));

        MenuButtonImage.sprite = MenuResources.MenuIconSprite;

        // Add functionality to the back arrow
        var backArrowElement = menuTransform.Find("button_BackArrow").GetComponent<FunctionElement>();
        backArrowElement.Do(OnBackArrowPressed);

        // Finally, populate the functions for all of the elements
        MenuPageHelper.PopulatePages(menuGameObject);
        MenuToolbarHelper.PopulateToolbar(menuTransform.Find("Popups/grid_Toolbar").gameObject);
    }

    private static void OnBackArrowPressed()
    {
        var selectedPage = MenuPage.SelectedPage;

        GoBack(selectedPage);
    }

    private static void GoBack(MenuPage page)
    {
        var parent = page.Parent;

        if (parent != null && parent.Parent != null)
        {
            var upperParent = parent.Parent;

            upperParent.SelectSubPage(parent);

            // Edge case fix
            // Could be avoided if I designed MenuPages better with FieldInjection but I do not care enough
            if (upperParent.SubPages.Count <= 1 && upperParent.Parent != null)
            {
                upperParent.Parent.SelectSubPage(upperParent);
            }

            return;
        }

        UIRig.Instance.popUpMenu.preferencesPanelView.PAGESELECT(0);
    }

    private static int InjectPage(PreferencesPanelView panelView, GameObject page)
    {
        var length = panelView.pages.Length + 1;
        var newPages = new Il2CppReferenceArray<GameObject>(length);

        for (var i = 0; i < panelView.pages.Length; i++)
        {
            newPages[i] = panelView.pages[i];
        }

        var newIndex = length - 1;
        newPages[newIndex] = page;

        panelView.pages = newPages;

        return newIndex;
    }
}