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
            ContactsList.ReadFile();
            PointSaveManager.ReadFile();

            AchievementSaveManager.OnInitializeMelon();
            AchievementSaveManager.ReadFile();

            ChangelogLoader.ReadFile();

            ModBlacklist.OnInitializeMelon();
            ModBlacklist.ReadFile();
        }
    }
}
