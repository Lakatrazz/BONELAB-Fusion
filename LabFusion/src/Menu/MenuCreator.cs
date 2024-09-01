using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using LabFusion.Marrow;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuCreator
{
    public static void CreateMenu()
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

        InjectPage(panelView, menuGameObject);
    }

    private static void InjectPage(PreferencesPanelView panelView, GameObject page)
    {
        var length = panelView.pages.Length + 1;
        var newPages = new Il2CppReferenceArray<GameObject>(length);

        for (var i = 0; i < panelView.pages.Length; i++)
        {
            newPages[i] = panelView.pages[i];
        }

        newPages[length - 1] = page;

        panelView.pages = newPages;
    }
}