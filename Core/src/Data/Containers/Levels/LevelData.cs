using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Data {
    public static class LevelData {
        public static void OnSceneAwake() {
            MineDiveData.OnSceneAwake();
        }

        public static void OnMainSceneInitialized() {
            MineDiveData.OnCacheInfo();
            MagmaGateData.OnCacheInfo();
            HubData.OnCacheInfo();
            KartRaceData.OnCacheInfo();
            HomeData.OnCacheInfo();
            DescentData.OnCacheInfo();
            ArenaData.OnCacheInfo();
            SprintBridgeData.OnCacheInfo();
            TimeTrialData.OnCacheInfo();
            GameControllerData.OnCacheInfo();
            VoidG114Data.OnCacheInfo();
        }
    }
}
