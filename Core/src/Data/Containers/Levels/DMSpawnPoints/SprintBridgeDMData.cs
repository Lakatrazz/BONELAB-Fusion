using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class SprintBridgeDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[8] {
    new Vector3(2.3381f, 31.5374f, 134.9037f),
    new Vector3(-0.4947f, 32.0374f, 94.5794f),
    new Vector3(14.106f, 32.0373f, 79.3274f),
    new Vector3(56.818f, 32.0373f, 81.8022f),
    new Vector3(-4.9095f, 33.971f, 68.9549f),
    new Vector3(0.169f, 31.5374f, 25.0182f),
    new Vector3(-1.2044f, 24.0373f, -13.917f),
    new Vector3(-0.2945f, 18.5374f, 179.6593f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "07 - Sprint Bridge 04" && FusionSceneManager.Level.Pallet.Internal) {
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
