using LabFusion.Preferences;

using UnityEngine;

using BoneLib.BoneMenu;

namespace LabFusion.BoneMenu;

using Menu = BoneLib.BoneMenu.Menu;

public static partial class BoneMenuCreator
{
    private static Page _mainPage = null;

    public static void OnPrepareMainPage()
    {
        _mainPage = Page.Root.CreatePage("Fusion", Color.white);
    }

    public static void OpenMainPage()
    {
        Menu.OpenPage(_mainPage);
    }

    public static void OnPopulateMainPage()
    {
        // Clear page
        _mainPage.RemoveAll();

        // Setup the sub pages
        CreateUniversalMenus(_mainPage);
    }

    private static void CreateUniversalMenus(Page page)
    {
        CreateNotificationsMenu(page);
    }
}