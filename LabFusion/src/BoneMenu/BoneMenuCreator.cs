using LabFusion.Preferences;
using LabFusion.Player;
using LabFusion.Marrow;

using UnityEngine;

using BoneLib.BoneMenu;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.BoneMenu;

using Menu = BoneLib.BoneMenu.Menu;

public static partial class BoneMenuCreator
{
    public static void RemoveEmptyPage(Page parent, Page child, Element link)
    {
        if (child.Elements.Count <= 0)
        {
            parent.Remove(link);
        }
    }

    #region MENU CATEGORIES
    public static void CreateFloatPreference(Page page, string name, float increment, float minValue, float maxValue, IFusionPref<float> pref)
    {
        var element = page.CreateFloat(name, Color.white, startingValue: pref.Value, increment: increment, minValue: minValue, maxValue: maxValue, callback: (v) =>
        {
            pref.Value = v;
        });

        pref.OnValueChanged += (v) =>
        {
            element.Value = v;
        };
    }

    public static void CreateBoolPreference(Page page, string name, IFusionPref<bool> pref)
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

        ScannableEvents.OnPalletAddedEvent += OnPalletAdded;
    }

    private static void OnPalletAdded(Barcode barcode)
    {
        // Check if the pallet is Fusion Content
        // If so, repopulate the matchmaking tab
        if (barcode == FusionPalletReferences.FusionContentReference.Barcode)
        {
            MatchmakingCreator.RecreatePage();
        }
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
        MatchmakingCreator.CreateMatchmakingPage(page);

        CreateGamemodesMenu(page);
        CreateSettingsMenu(page);
        CreateNotificationsMenu(page);
        CreateBanListMenu(page);
    }
}