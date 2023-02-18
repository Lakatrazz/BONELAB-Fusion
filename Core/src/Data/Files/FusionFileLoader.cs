using LabFusion.SDK.Points;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Data {
    public static class FusionFileLoader {
        public static void OnInitializeMelon() {
            PermissionList.PullFromFile();
            BanList.PullFromFile();
            PointSaveManager.ReadFromFile();
        }

        public static void OnDeinitializeMelon() {
            PointSaveManager.WriteBackup();
        }
    }
}
