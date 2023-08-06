using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class MonoWayDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[14] {
    new Vector3(49.9405f, 1.0374f, -51.5276f),
    new Vector3(39.0956f, 7.0375f, -24.3985f),
    new Vector3(34.3241f, 1.197f, -52.855f),
    new Vector3(-32.3943f, 10.9575f, 4.8938f),
    new Vector3(24.2696f, 4.1897f, 64.7375f),
    new Vector3(78.0983f, 6.4815f, 55.0796f),
    new Vector3(89.998f, 3.6803f, -2.1484f),
    new Vector3(43.7032f, 14.0374f, 14.1218f),
    new Vector3(2.5037f, 6.0376f, 68.6963f),
    new Vector3(26.7221f, 5.6433f, 24.9881f),
    new Vector3(-26.8878f, 6.1773f, -62.3645f),
    new Vector3(31.9522f, 0.3404f, -61.9676f),
    new Vector3(10.5809f, 7.5374f, -34.0675f),
    new Vector3(80.5201f, 7.2038f, -67.1719f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "10 - Monogon Motorway" && FusionSceneManager.Level.Pallet.Internal) {
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
