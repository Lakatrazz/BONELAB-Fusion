using LabFusion.SDK.Achievements;
using LabFusion.SDK.Points;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Data {
    public static class FusionFileLoader {
        public static void OnInitializeMelon() {
            PermissionList.ReadFile();
            BanList.ReadFile();
            ContactsList.ReadFile();
            PointSaveManager.ReadFile();

            AchievementSaveManager.OnInitializeMelon();
            AchievementSaveManager.ReadFile();

            ChangelogLoader.ReadFile();
        }

        public static void OnDeinitializeMelon() {
            PointSaveManager.WriteBackup();
            AchievementSaveManager.WriteBackup();
        }
    }
}
