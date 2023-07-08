﻿using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class MirrorData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[8] {
    new Vector3(-0.3258f, 1.3078f, 2.3584f),
    new Vector3(-12.8975f, 1.3411f, -12.7348f),
    new Vector3(-11.0187f, 1.2294f, 0.2149f),
    new Vector3(0.1774f, 1.2333f, -10.9054f),
    new Vector3(-13.9262f, 10.2407f, -14.7184f),
    new Vector3(-11.6317f, 10.1571f, -0.3718f),
    new Vector3(-1.2534f, 10.245f, -9.7245f),
    new Vector3(-1.3073f, 10.8671f, -0.2344f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "Mirror" && FusionSceneManager.Level.Pallet.Internal) {
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
