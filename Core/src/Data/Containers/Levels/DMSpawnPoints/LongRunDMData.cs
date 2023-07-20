﻿using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class LongRunDMData : LevelDataHandler {
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[7] {
    new Vector3(36.1693f, -21.9632f, -148.1691f),
    new Vector3(36.2307f, -22.4638f, -138.8987f),
    new Vector3(27.9539f, -32.9626f, -146.4428f),
    new Vector3(35.8871f, -32.9626f, -130.9124f),
    new Vector3(28.6148f, -23.9616f, -163.052f),
    new Vector3(32.2719f, -29.9626f, -104.5819f),
    new Vector3(32.0926f, -32.8966f, -143.0124f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "03 - LongRun" && FusionSceneManager.Level.Pallet.Internal) {
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
