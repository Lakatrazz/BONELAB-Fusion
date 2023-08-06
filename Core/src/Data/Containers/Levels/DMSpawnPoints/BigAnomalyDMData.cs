using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class BigAnomalyDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[9] {
    new Vector3(4.4089f, 40.0375f, 108.1976f),
    new Vector3(27.662f, 43.0375f, 93.3687f),
    new Vector3(-9.566f, 40.0375f, 103.2718f),
    new Vector3(27.6981f, 20.0375f, 94.1142f),
    new Vector3(32.4794f, 25.0374f, 97.0537f),
    new Vector3(37.5303f, 25.0374f, 84.4273f),
    new Vector3(26.2376f, 28.0375f, 103.2391f),
    new Vector3(41.3916f, 25.0374f, 60.6696f),
    new Vector3(29.4577f, 25.0374f, 61.6327f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "05 - Big Anomaly" && FusionSceneManager.Level.Pallet.Internal) {
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
