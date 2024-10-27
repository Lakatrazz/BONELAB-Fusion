using LabFusion.SDK.Achievements;
using LabFusion.SDK.Points;

namespace LabFusion.Data
{
    public static class FusionFileLoader
    {
        public static void OnInitializeMelon()
        {
            PermissionList.ReadFile();
            BanManager.ReadFile();
            ContactsList.ReadFile();
            PointSaveManager.ReadFile();

            AchievementSaveManager.OnInitializeMelon();
            AchievementSaveManager.ReadFile();

            ChangelogLoader.ReadFile();
        }

        public static void OnDeinitializeMelon()
        {
            PointSaveManager.WriteBackup();
            AchievementSaveManager.WriteBackup();
        }
    }
}
