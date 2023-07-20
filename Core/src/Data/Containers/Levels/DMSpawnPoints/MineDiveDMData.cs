using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class MineDiveDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[6] {
    new Vector3(-5.6735f, -0.0314f, -11.8826f),
    new Vector3(-10.8307f, -0.0054f, 11.0599f),
    new Vector3(-11.1408f, 0.3384f, -23.1127f),
    new Vector3(-5.2951f, -0.1026f, -34.264f),
    new Vector3(-1.4603f, -0.0231f, -24.2549f),
    new Vector3(3.4105f, -0.0699f, 10.5926f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "04 - Mine Dive" && FusionSceneManager.Level.Pallet.Internal) {
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
