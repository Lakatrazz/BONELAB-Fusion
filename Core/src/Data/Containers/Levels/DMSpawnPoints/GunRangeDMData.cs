﻿using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class GunRangeDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[8] {
    new Vector3(-17.3122f, 0.0374f, -48.4204f),
    new Vector3(-1.2045f, 0.0374f, -30.4701f),
    new Vector3(-1.2281f, 0.0374f, -41.9648f),
    new Vector3(-1.4794f, 0.0461f, -18.6144f),
    new Vector3(-9.3656f, 0.0374f, -7.2933f),
    new Vector3(-16.5774f, 0.0374f, -11.0135f),
    new Vector3(-7.7083f, 0.0374f, -24.4492f),
    new Vector3(-13.5419f, 0.0374f, -35.0949f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "Gun Range" && FusionSceneManager.Level.Pallet.Internal) {
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
