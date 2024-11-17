using UnityEngine;

using LabFusion.Menu.Gamemodes;

namespace LabFusion.Menu;

public static class MenuPageHelper
{
    public static void OnInitializeMelon()
    {
        MenuLocation.OnInitializeMelon();
        MenuGamemode.OnInitializeMelon();
    }

    public static void PopulatePages(GameObject root)
    {
        MenuProfile.PopulateProfile(root.transform.Find("page_Profile").gameObject);
        MenuLocation.PopulateLocation(root.transform.Find("page_Location").gameObject);
        MenuMatchmaking.PopulateMatchmaking(root.transform.Find("page_Matchmaking").gameObject);
        MenuSettings.PopulateSettings(root.transform.Find("page_Settings").gameObject);
        MenuGamemode.PopulateGamemode(root.transform.Find("page_Gamemode").gameObject);
        MenuNotifications.PopulateNotifications(root.transform.Find("page_Notifications").gameObject);
    }
}