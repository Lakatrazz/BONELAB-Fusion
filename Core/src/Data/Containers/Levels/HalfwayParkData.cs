using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class HalfwayParkData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[5] {
            new Vector3(-85.0378f, 15.1181f, 85.4412f),
            new Vector3(9.0977f, 9.117f, 1.0171f),
            new Vector3(-49.9463f, 11.1174f, 85.6447f),
            new Vector3(-36.0197f, -5.8826f, 54.2049f),
            new Vector3(-79.8466f, -1.8826f, -9.0016f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is halfway park
            if (FusionSceneManager.Title == "Halfway Park" && FusionSceneManager.Level.Pallet.Internal) {
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
