using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class DropPitDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[8] {
    new Vector3(-19.6002f, -2.4644f, -7.0592f),
    new Vector3(-14.2666f, -0.4644f, 2.0659f),
    new Vector3(-14.7582f, -0.4626f, -21.4207f),
    new Vector3(-15.1494f, -0.4644f, -10.5613f),
    new Vector3(-10.0619f, -0.4644f, -10.7547f),
    new Vector3(-8.8003f, -2.4644f, -11.9671f),
    new Vector3(-19.1125f, -0.4626f, -21.8052f),
    new Vector3(-6.1433f, -0.4626f, -22.927f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "Drop Pit" && FusionSceneManager.Level.Pallet.Internal) {
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
