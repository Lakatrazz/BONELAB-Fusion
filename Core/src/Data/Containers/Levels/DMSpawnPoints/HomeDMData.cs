using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class HomeDMData : LevelDataHandler {//theres a good chance that nobody else will read this comment, or even play on this map
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[8] {
    new Vector3(-1.12f, 12.018f, -48.6185f),
    new Vector3(-9.0939f, -5.2375f, -71.086f),
    new Vector3(12.9752f, -5.2126f, -51.2283f),
    new Vector3(2.0007f, -3.6254f, -56.3616f),
    new Vector3(-19.5654f, -5.2125f, -79.5254f),
    new Vector3(-20.3357f, -5.2125f, -46.3607f),
    new Vector3(18.6373f, 4.7875f, -43.5794f),
    new Vector3(9.7098f, -5.0691f, -77.7367f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "14 - Home" && FusionSceneManager.Level.Pallet.Internal) {
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
