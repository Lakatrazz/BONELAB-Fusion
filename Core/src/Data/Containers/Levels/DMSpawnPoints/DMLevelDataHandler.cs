using LabFusion.MarrowIntegration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public abstract class DMLevelDataHandler : LevelDataHandler {
        protected abstract Vector3[] DeathmatchSpawnPoints { get; }

        protected override void MainSceneInitialized() {
            // Create DM spawn points
            for (var i = 0; i < DeathmatchSpawnPoints.Length; i++) {
                GameObject spawnPoint = new("Deathmatch Spawn");
                spawnPoint.transform.position = DeathmatchSpawnPoints[i];
                spawnPoint.AddComponent<DeathmatchSpawnpoint>();
                spawnPoint.AddComponent<LavaGangSpawnpoint>();
                spawnPoint.AddComponent<SabrelakeSpawnpoint>();
            }
        }
    }
}
