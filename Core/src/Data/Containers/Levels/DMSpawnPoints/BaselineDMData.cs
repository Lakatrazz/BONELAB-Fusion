using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class BaselineDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[5] {
    new Vector3(-38.7547f, 0.0375f, 38.9284f),
    new Vector3(38.0896f, 0.0374f, 38.1792f),
    new Vector3(35.7948f, -1.9619f, -45.7488f),
    new Vector3(-38.8625f, 0.0377f, -38.8162f),
    new Vector3(-2.0933f, 0.0374f, -3.9311f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "Baseline" && FusionSceneManager.Level.Pallet.Internal) {
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
