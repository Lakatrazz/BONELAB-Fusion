using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class MagmaGateDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[15] {
    new Vector3(-37.7922f, -0.1129f, 34.9711f),
    new Vector3(-14.8091f, 0.9308f, 35.5413f),
    new Vector3(4.7724f, 3.0374f, -0.7067f),
    new Vector3(7.6075f, 10.0373f, 1.4012f),
    new Vector3(-2.6123f, 12.0374f, 55.714f),
    new Vector3(-15.5539f, 6.0374f, 53.5095f),
    new Vector3(0.1097f, 12.0053f, 27.9264f),
    new Vector3(-23.6288f, 3.0374f, 0.9346f),
    new Vector3(-19.8211f, 15.0374f, 1.8029f),
    new Vector3(-14.2566f, 13.0374f, 52.8782f),
    new Vector3(16.1856f, 13.0374f, 53.5135f),
    new Vector3(14.9417f, 32.0374f, 57.1784f),
    new Vector3(6.3328f, 30.0374f, -0.4239f),
    new Vector3(-7.9078f, 12.0374f, -2.0455f),
    new Vector3(9.312f, 4.9901f, 29.5975f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "01 - Descent" && FusionSceneManager.Level.Pallet.Internal) {
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
