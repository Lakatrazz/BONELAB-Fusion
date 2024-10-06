using UnityEngine;

namespace LabFusion.Menu;

public static class MenuPageHelper
{
    public static void OnInitializeMelon()
    {
        MenuLocation.OnInitializeMelon();
    }

    public static void PopulatePages(GameObject root)
    {
        MenuProfile.PopulateProfile(root.transform.Find("page_Profile").gameObject);
        MenuLocation.PopulateLocation(root.transform.Find("page_Location").gameObject);
    }
}