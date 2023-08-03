using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class DungWarDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[7] {//yipeeeeeeeeeeee the final oneeeeeee
    new Vector3(54.2045f, -7.5652f, -65.2812f),
    new Vector3(62.5462f, 9.3181f, -77.1614f),
    new Vector3(49.9896f, 11.2435f, -75.2906f),
    new Vector3(62.4793f, 3.2703f, -75.1372f),
    new Vector3(58.0072f, 19.0727f, -71.7206f),
    new Vector3(54.3207f, -7.8262f, -73.7793f),
    new Vector3(58.6233f, 21.2966f, -74.2617f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "Dungeon Warrior" && FusionSceneManager.Level.Pallet.Internal) {
                // Create DM spawn points
                for (var i = 0; i < _deathmatchSpawnPoints.Length; i++) {
                    GameObject spawnPoint = new GameObject("Deathmatch Spawn");
                    spawnPoint.transform.position = _deathmatchSpawnPoints[i];
                    spawnPoint.AddComponent<DeathmatchSpawnpoint>();
                    spawnPoint.AddComponent<LavaGangSpawnpoint>();
                    spawnPoint.AddComponent<SabrelakeSpawnpoint>();
                }
            }
        }
    }
}
