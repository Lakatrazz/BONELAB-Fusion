using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class BigBoneBowlingDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[11] {
    new Vector3(16.2324f, -25.712f, 61.4863f),
    new Vector3(-21.725f, -25.712f, 24.8524f),
    new Vector3(-15.5071f, -25.7122f, 54.1341f),
    new Vector3(-21.2391f, -25.7125f, 99.1708f),
    new Vector3(15.4044f, -25.7121f, 81.8368f),
    new Vector3(48.8102f, -25.7124f, 81.8609f),
    new Vector3(53.8504f, -25.7125f, 99.2813f),
    new Vector3(53.2528f, -25.712f, 61.801f),
    new Vector3(48.7699f, -25.7121f, 29.7422f),
    new Vector3(24.4213f, -25.7121f, 30.5197f),
    new Vector3(-4.5168f, -25.7123f, 70.1425f)),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "Big Bone Bowling" && FusionSceneManager.Level.Pallet.Internal) {
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
