using LabFusion.Preferences;

using UnityEngine;

using BoneLib.BoneMenu;

namespace LabFusion.BoneMenu;

using Menu = BoneLib.BoneMenu.Menu;

public static partial class BoneMenuCreator
{
    #region MENU CATEGORIES
    public static void CreateBoolPreference(Page page, string name, FusionPref<bool> pref)
    {
        var element = page.CreateBool(name, Color.white, pref.Value, (v) =>
        {
            pref.Value = v;
        });

        pref.OnValueChanged += (v) =>
        {
            element.Value = v;
        };
    }
    #endregion

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
        CreateGamemodesMenu(page);
        CreateNotificationsMenu(page);
    }
}