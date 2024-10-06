using UnityEngine;

namespace LabFusion.Menu;

public static class MenuPageHelper
{
    public static void PopulatePages(GameObject root)
    {
        MenuProfile.PopulateProfile(root.transform.Find("page_Profile").gameObject);
    }
}