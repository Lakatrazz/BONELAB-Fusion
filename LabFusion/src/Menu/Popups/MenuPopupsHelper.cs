using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuPopupsHelper
{
    public static GameObject PopupsRoot { get; private set; } = null;

    public static void OnInitializeMelon()
    {
        MenuToolbarHelper.OnInitializeMelon();

        NetworkLayerManager.OnLoggedInChanged += OnLoggedInChanged;
    }

    public static void PopulatePopups(GameObject popups)
    {
        PopupsRoot = popups;

        MenuToolbarHelper.PopulateToolbar(popups.transform.Find("grid_Toolbar").gameObject);

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
        if (PopupsRoot == null)
        {
            return;
        }

        PopupsRoot.SetActive(true);
    }

    private static void OnLoggedOut()
    {
        if (PopupsRoot == null)
        {
            return;
        }

        PopupsRoot.SetActive(false);
    }
}
