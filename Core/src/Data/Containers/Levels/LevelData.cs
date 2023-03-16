using LabFusion.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Data {
    public static class LevelData {
        public static void OnSceneAwake() {
            MineDiveData.OnSceneAwake();
        }

        public static void OnMainSceneInitialized() {
            // Check info for every level
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
            HolodeckData.OnCacheInfo();

            // Apply universal scene changes
            if (NetworkInfo.HasServer) {
                // Get all scene gameobjects
                var gameObjects = GameObject.FindObjectsOfType<GameObject>();

                for (var i = 0; i < gameObjects.Length; i++) {
                    var go = gameObjects[i];

                    // Get name
                    string name = go.name;

                    // Reload scene/hub buttons
                    if (name.Contains("prop_bigButton_LOADHUB") || name.Contains("prop_bigButton_RESET"))
                        go.SetActive(false);
                }
            }
        }
    }
}
