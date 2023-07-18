using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class MainMenUData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[4] {
    new Vector3(29.8048f, -1.1377f, 0.8369f),
    new Vector3(26.6286f, -1.1377f, 0.9376f),
    new Vector3(29.5723f, -1.1377f, -1.0273f),
    new Vector3(26.4974f, -1.1377f, -1.1975f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "00 - Main Menu" && FusionSceneManager.Level.Pallet.Internal) {
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
