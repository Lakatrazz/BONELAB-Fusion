using LabFusion.Preferences;
using LabFusion.Preferences.Client;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.Marrow;

using UnityEngine;

using BoneLib.BoneMenu;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.BoneMenu;

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
    public static void CreateColorPreference(Page page, IFusionPref<Color> pref)
    {
        var currentColor = pref;
        var colorR = page.CreateFloat("Red", Color.red, startingValue: currentColor.Value.r, increment: 0.05f, minValue: 0f, maxValue: 1f, callback: (r) =>
        {
            r = Mathf.Round(r * 100f) / 100f;
            var color = currentColor.Value;
            color.r = r;
            currentColor.Value = color;
        });
        var colorG = page.CreateFloat("Green", Color.green, startingValue: currentColor.Value.g, increment: 0.05f, minValue: 0f, maxValue: 1f, callback: (g) =>
        {
            g = Mathf.Round(g * 100f) / 100f;
            var color = currentColor.Value;
            color.g = g;
            currentColor.Value = color;
        });
        var colorB = page.CreateFloat("Blue", Color.blue, startingValue: currentColor.Value.b, increment: 0.05f, minValue: 0f, maxValue: 1f, callback: (b) =>
        {
            b = Mathf.Round(b * 100f) / 100f;
            var color = currentColor.Value;
            color.b = b;
            currentColor.Value = color;
        });
        var colorPreview = page.CreateFunction("■■■■■■■■■■■", currentColor.Value, null);

        currentColor.OnValueChanged += (color) =>
        {
            colorR.Value = color.r;
            colorG.Value = color.g;
            colorB.Value = color.b;
            colorPreview.ElementColor = color;
        };
    }

    public static void CreateBytePreference(Page page, string name, byte increment, byte minValue, byte maxValue, IFusionPref<byte> pref)
    {
        var element = page.CreateInt(name, Color.white, startingValue: pref.Value, increment: increment, minValue: minValue, maxValue: maxValue, callback: (v) =>
        {
            pref.Value = (byte)v;
        });

        pref.OnValueChanged += (v) =>
        {
            element.Value = v;
        };
    }

    public static void CreateIntPreference(Page page, string name, int increment, int minValue, int maxValue, IFusionPref<int> pref)
    {
        var element = page.CreateInt(name, Color.white, startingValue: pref.Value, increment: increment, minValue: minValue, maxValue: maxValue, callback: (v) =>
        {
            pref.Value = v;
        });

        pref.OnValueChanged += (v) =>
        {
            element.Value = v;
        };
    }

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

    public static void CreateEnumPreference<TEnum>(Page page, string name, IFusionPref<TEnum> pref) where TEnum : Enum
    {
        var element = page.CreateEnum(name, Color.white, pref.Value, (v) =>
        {
            pref.Value = (TEnum)v;
        });

        pref.OnValueChanged += (v) =>
        {
            element.Value = v;
        };
    }

    public static void CreateStringPreference(Page page, string name, IFusionPref<string> pref, Action<string> onValueChanged = null, int maxLength = PlayerIdManager.MaxNameLength)
    {
        string currentValue = pref.Value;
        var element = page.CreateString(name, Color.white, currentValue, (v) =>
        {
            pref.Value = v;
        });
        
        pref.OnValueChanged += (v) =>
        {
            element.Value = v;

            onValueChanged?.Invoke(v);
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

    private static int _lastIndex;

    public static void OnPopulateMainPage()
    {
        // Clear page
        _mainPage.RemoveAll();

        // Create category for changing network layer
        var networkLayerManager = _mainPage.CreatePage("Network Layer Manager", Color.yellow);
        var func = networkLayerManager.CreateFunction("Players need to be on the same layer!", Color.yellow, null);
        
        _lastIndex = NetworkLayer.SupportedLayers.IndexOf(NetworkLayerDeterminer.LoadedLayer);

        networkLayerManager.CreateFunction($"Active Layer: {NetworkLayerDeterminer.LoadedTitle}", Color.white, null);
        
        var changeLayerCategory = networkLayerManager.CreatePage("Change Layer", Color.white);

        var targetPanel = changeLayerCategory.CreateFunction($"Target Layer: {ClientSettings.NetworkLayerTitle.Value}", Color.white, null);
        changeLayerCategory.CreateFunction("Cycle", Color.white, () =>
        {
            int count = NetworkLayer.SupportedLayers.Count;
            if (count <= 0)
                return;

            _lastIndex++;
            if (count <= _lastIndex)
                _lastIndex = 0;

            ClientSettings.NetworkLayerTitle.Value = NetworkLayer.SupportedLayers[_lastIndex].Title;
        });
        ClientSettings.NetworkLayerTitle.OnValueChanged += (v) =>
        {
            targetPanel.ElementName = $"Target Layer: {v}";
        };

        changeLayerCategory.CreateFunction("SET NETWORK LAYER", Color.green, () => InternalLayerHelpers.UpdateLoadedLayer());

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
        CreateDownloadingMenu(page);

#if DEBUG
        // Debug only (dev tools)
        CreateDebugMenu(page);
#endif
    }
}