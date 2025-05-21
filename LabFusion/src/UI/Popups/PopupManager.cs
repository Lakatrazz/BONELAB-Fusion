namespace LabFusion.UI.Popups;

public static class PopupManager
{
    internal static void OnInitializeMelon()
    {
        AchievementPopup.OnInitializeMelon();
    }

    internal static void OnUpdate()
    {
        Notifier.OnUpdate();
        AchievementPopup.OnUpdate();
        BitPopup.OnUpdate();
    }
}
