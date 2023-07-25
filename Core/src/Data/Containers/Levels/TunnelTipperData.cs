using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class TunnelTipperData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[10] {
    new Vector3(10.0078f, 16.0968f, 21.5753f),
    new Vector3(10.0832f, 15.663f, 25.6005f),
    new Vector3(9.9641f, 18.1093f, -20.7673f),
    new Vector3(10.595f, 15.1221f, -15.0623f),
    new Vector3(11.1202f, 1.1871f, 10.3299f),
    new Vector3(6.7449f, 6.9961f, 2.9742f),
    new Vector3(11.9569f, 7.818f, -4.9224f),
    new Vector3(9.2211f, -9.798f, 26.545f),
    new Vector3(9.386f, 9.9136f, -15.1336f),
    new Vector3(9.7402f, 10.0533f, 18.4418f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "Tunnel Tipper" && FusionSceneManager.Level.Pallet.Internal) {
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
