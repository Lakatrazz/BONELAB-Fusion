using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class ContainerYardData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[8] {
    new Vector3(28.0497f, -0.2327f, 0.0443f),
    new Vector3(27.741f, 4.8885f, -5.717f),
    new Vector3(27.5797f, 4.6144f, -6.9054f),
    new Vector3(28.0605f, -0.1453f, 4.4828f),
    new Vector3(39.0173f, -0.7884f, 15.841f),
    new Vector3(13.4692f, -0.8171f, 9.5978f),
    new Vector3(18.9537f, -0.8847f, -10.8288f),
    new Vector3(35.9514f, -0.8819f, -12.1057f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "15 - Void G114" && FusionSceneManager.Level.Pallet.Internal) {
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
