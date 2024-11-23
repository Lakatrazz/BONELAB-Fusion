using UnityEngine;

using LabFusion.Menu.Gamemodes;
using LabFusion.Marrow.Proxies;

namespace LabFusion.Menu;

public static class MenuPageHelper
{
    public static MenuPage RootPage { get; private set; } = null;

    public static FunctionElement LogOutElement { get; private set; } = null;

    public static void OnInitializeMelon()
    {
        MenuLocation.OnInitializeMelon();
        MenuGamemode.OnInitializeMelon();
    }

    public static void PopulatePages(GameObject root)
    {
        RootPage = root.GetComponent<MenuPage>();

        var transform = root.transform;

        MenuProfile.PopulateProfile(transform.Find("page_Profile").gameObject);
        MenuLocation.PopulateLocation(transform.Find("page_Location").gameObject);
        MenuMatchmaking.PopulateMatchmaking(transform.Find("page_Matchmaking").gameObject);
        MenuSettings.PopulateSettings(transform.Find("page_Settings").gameObject);
        MenuGamemode.PopulateGamemode(transform.Find("page_Gamemode").gameObject);
        MenuNotifications.PopulateNotifications(transform.Find("page_Notifications").gameObject);
        MenuLogIn.PopulateLogIn(transform.Find("page_LogIn").gameObject);

        LogOutElement = transform.Find("button_LogOut").GetComponent<FunctionElement>()
            .Do(OnLogOutPressed);

        OnLoggedOut();
    }

    private static void OnLoggedIn()
    {
        if (RootPage == null)
        {
            return;
        }

        RootPage.DefaultPageIndex = 0;
        LogOutElement.gameObject.SetActive(true);
    }

    private static void OnLoggedOut()
    {
        if (RootPage == null)
        {
            return;
        }

        RootPage.DefaultPageIndex = 6;
        LogOutElement.gameObject.SetActive(false);
    }

    private static void OnLogOutPressed()
    {

    }
}