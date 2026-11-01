using LabFusion.Contacts;
using LabFusion.Moderation;
using LabFusion.Permissions;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Points;

namespace LabFusion.Data
{
    public static class FusionFileLoader
    {
        public static void OnInitializeMelon()
        {
            PermissionManager.ReadFile();
            BanManager.ReadFile();
            ContactManager.ReadFile();
            PointSaveManager.ReadFile();

            AchievementSaveManager.OnInitializeMelon();
            AchievementSaveManager.ReadFile();

            ChangelogLoader.ReadFile();

            ModBlacklist.OnInitializeMelon();
            ModBlacklist.ReadFile();
        }
    }
}
