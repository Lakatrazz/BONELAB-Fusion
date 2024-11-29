using UnityEngine;

using LabFusion.Menu.Gamemodes;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Preferences.Client;

namespace LabFusion.Menu;

public static class MenuPageHelper
{
    public static Vector3 DefaultScale { get; private set; } = Vector3.one;

    public static Transform MenuParent { get; private set; } = null;

    public static MenuPage RootPage { get; private set; } = null;

    public static FunctionElement LogOutElement { get; private set; } = null;

    public static void OnInitializeMelon()
    {
        MenuProfile.OnInitializeMelon();
        MenuLocation.OnInitializeMelon();
        MenuGamemode.OnInitializeMelon();

        NetworkLayerManager.OnLoggedInChanged += OnLoggedInChanged;

        ClientSettings.MenuSize.OnValueChanged += OnMenuSizeChanged;
    }

    private static void OnMenuSizeChanged(float value)
    {
        if (RootPage == null || !RootPage.isActiveAndEnabled)
        {
            return;
        }

        MenuParent.localScale = DefaultScale * value;
    }

    private static void OnMenuOpened()
    {
        if (MenuParent == null)
        {
            return;
        }

        MenuParent.localScale = DefaultScale * ClientSettings.MenuSize.Value;
    }

    private static void OnMenuClosed()
    {
        if (MenuParent == null)
        {
            return;
        }

        MenuParent.localScale = DefaultScale;
    }

    public static void PopulatePages(GameObject root)
    {
        RootPage = root.GetComponent<MenuPage>();
        RootPage.OnEnabled += OnMenuOpened;
        RootPage.OnDisabled += OnMenuClosed;

        var transform = root.transform;

        MenuParent = transform.parent;
        DefaultScale = MenuParent.localScale;

        MenuProfile.PopulateProfile(transform.Find("page_Profile").gameObject);
        MenuLocation.PopulateLocation(transform.Find("page_Location").gameObject);
        MenuMatchmaking.PopulateMatchmaking(transform.Find("page_Matchmaking").gameObject);
        MenuSettings.PopulateSettings(transform.Find("page_Settings").gameObject);
        MenuGamemode.PopulateGamemode(transform.Find("page_Gamemode").gameObject);
        MenuNotifications.PopulateNotifications(transform.Find("page_Notifications").gameObject);
        MenuLogIn.PopulateLogIn(transform.Find("page_LogIn").gameObject);

        LogOutElement = transform.Find("button_LogOut").GetComponent<FunctionElement>()
            .Do(OnLogOutPressed);

        UpdateLogIn();
    }

    private static void OnLoggedInChanged(bool value)
    {
        UpdateLogIn();
    }

    private static void UpdateLogIn()
    {
        if (NetworkLayerManager.LoggedIn)
        {
            OnLoggedIn();
        }
        else
        {
            OnLoggedOut();
        }
    }

    private static void OnLoggedIn()
    {
        if (RootPage == null)
        {
            return;
        }

        RootPage.DefaultPageIndex = 0;
        RootPage.SelectSubPage(0);

        LogOutElement.gameObject.SetActive(true);
    }

    private static void OnLoggedOut()
    {
        if (RootPage == null)
        {
            return;
        }

        RootPage.DefaultPageIndex = 6;
        RootPage.SelectSubPage(6);

        LogOutElement.gameObject.SetActive(false);
    }

    private static void OnLogOutPressed()
    {
        NetworkLayerManager.LogOut();
    }
}