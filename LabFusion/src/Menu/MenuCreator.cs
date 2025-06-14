using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;

using LabFusion.Downloading;
using LabFusion.Downloading.ModIO;
using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;
using LabFusion.Utilities;
using LabFusion.UI.Popups;
using LabFusion.Marrow.Pool;

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Menu;

public static class MenuCreator
{
    public static GameObject MenuGameObject { get; private set; } = null;

    public static int MenuPageIndex => _menuPageIndex;
    private static int _menuPageIndex = -1;

    public static Image MenuButtonImage => _menuButtonImage;
    private static Image _menuButtonImage = null;

    public static MenuPopups MenuPopups => _menuPopups;
    private static MenuPopups _menuPopups = null;

    public static void OnInitializeMelon()
    {
        MenuPageHelper.OnInitializeMelon();
        MenuPopupsHelper.OnInitializeMelon();
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
        buttonScript.onClick = new Button.ButtonClickedEvent();
        buttonScript.AddClickEvent(OnMenuButtonClicked);

        // Cache references
        _menuButtonImage = fusionButton.transform.Find("img_icon").GetComponent<Image>();
    }

    private static bool _isDownloadingContent = false;

    private static bool CheckFusionContent()
    {
        if (_isDownloadingContent)
        {
            return false;
        }

        var contentStatus = FusionPalletReferences.ValidateContentPallet();

        switch (contentStatus)
        {
            default:
            case FusionPalletReferences.PalletStatus.MISSING:
                Notifier.Send(new Notification()
                {
                    Title = "Missing Fusion Content",
                    Message = "The Fusion Content mod is missing! Beginning download...",
                    Type = NotificationType.INFORMATION,
                    SaveToMenu = false,
                    ShowPopup = true,
                    PopupLength = 4f,
                });

                DownloadContent();
                return false;
            case FusionPalletReferences.PalletStatus.OUTDATED:
                Notifier.Send(new Notification()
                {
                    Title = "Outdated Fusion Content",
                    Message = "The installed Fusion Content mod is outdated! Updating...",
                    Type = NotificationType.INFORMATION,
                    SaveToMenu = false,
                    ShowPopup = true,
                    PopupLength = 4f,
                });

                DownloadContent();
                return false;
            case FusionPalletReferences.PalletStatus.FOUND:
                return true;
        }
    }

    private static void DownloadContent()
    {
        _isDownloadingContent = true;

        ModIODownloader.EnqueueDownload(new ModTransaction()
        {
            ModFile = new ModIOFile(ModReferences.FusionContentId),
            Callback = OnCallbackReceived,
        });

        static void OnCallbackReceived(DownloadCallbackInfo info)
        {
            _isDownloadingContent = false;

            if (info.result != ModResult.SUCCEEDED)
            {
                Notifier.Send(new Notification()
                {
                    Title = "Download Failed",
                    Message = "The Fusion Content failed to install! Make sure you are logged into mod.io in VoidG114 or BONELAB Hub!",
                    Type = NotificationType.ERROR,
                    SaveToMenu = false,
                    ShowPopup = true,
                    PopupLength = 4f,
                });

                return;
            }

            // Now that the pallet is loaded, spawn the menu
            SpawnMenu();
        }
    }

    private static void OnMenuButtonClicked()
    {
        if (!CheckFusionContent())
        {
            return;
        }

        // Make sure the page has been spawned properly
        if (_menuPageIndex < 0)
        {
            Notifier.Send(new Notification()
            {
                Title = "Failed to Open Menu",
                Message = "The Fusion menu does not exist! Please reinstall the Fusion Content mod.io mod!",
                Type = NotificationType.ERROR,
                SaveToMenu = false,
                ShowPopup = true,
                PopupLength = 6f,
            });

            FusionLogger.Error("Tried opening the menu, but it doesn't exist! Please reinstall the Fusion Content mod.io mod!");
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
        // Make sure the content is the right version
        if (FusionPalletReferences.ValidateContentPallet() != FusionPalletReferences.PalletStatus.FOUND)
        {
            return;
        }

        // Register and spawn the menu spawnable
        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.FusionMenuReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, OnMenuSpawned);
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
        MenuGameObject = poolee.gameObject;
        var menuTransform = poolee.transform;

        menuTransform.parent = panelView.transform;
        menuTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        menuTransform.localScale = Vector3.one;

        _menuPageIndex = InjectPage(panelView, MenuGameObject);

        // Cache references
        _menuPopups = MenuGameObject.GetComponentInChildren<MenuPopups>(true);

        // Setup text
        MenuButtonHelper.PopulateTexts(MenuGameObject);

        // Setup buttons
        MenuButtonHelper.PopulateButtons(MenuGameObject);

        // Get resources contained in the menu
        MenuResources.GetResources(menuTransform.Find("Resources"));

        MenuButtonImage.sprite = MenuResources.MenuIconSprite;

        // Add functionality to the back arrow
        var backArrowElement = menuTransform.Find("button_BackArrow").GetComponent<FunctionElement>();
        backArrowElement.Do(OnBackArrowPressed);

        // Finally, populate the functions for all of the elements
        MenuPageHelper.PopulatePages(MenuGameObject);
        MenuPopupsHelper.PopulatePopups(menuTransform.Find("Popups").gameObject);
    }

    private static void OnBackArrowPressed()
    {
        var selectedPage = MenuPageHelper.RootPage.RootCurrentPage;

        GoBack(selectedPage);
    }

    private static void GoBack(MenuPage page)
    {
        var parent = page.Parent;

        if (parent != null && parent.Parent != null)
        {
            var upperParent = parent.Parent;

            // Edge case fix
            // Could be avoided if I designed MenuPages better with FieldInjection but I do not care enough
            if ((upperParent.SubPages.Count <= 1 || parent.SubPages.IndexOf(parent.CurrentPage) == parent.DefaultPageIndex) && upperParent.Parent != null)
            {
                upperParent.Parent.SelectSubPage(upperParent);
            }
            else
            {
                upperParent.SelectSubPage(parent);
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