using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class PillarClimbDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[8] {
    new Vector3(3.7457f, 0.0374f, -1.5809f),
    new Vector3(0.8794f, 8.0174f, 1.4217f),
    new Vector3(0.3373f, 14.8141f, 0.2999f),
    new Vector3(3.9943f, 24.1792f, -0.2227f),
    new Vector3(15.8407f, 48.7708f, 6.105f),
    new Vector3(8.5758f, 35.8305f, -0.3652f),
    new Vector3(10.5365f, 49.3929f, -4.7714f),
    new Vector3(-4.1714f, 36.5375f, -0.3992f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "11 - Pillar Climb" && FusionSceneManager.Level.Pallet.Internal) {
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
