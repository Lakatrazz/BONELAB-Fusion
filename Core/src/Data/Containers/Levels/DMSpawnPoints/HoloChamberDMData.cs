using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class HoloChamberBDMData : LevelDataHandler {//this is basically smaller and more confined baseline........... 
        private static readonly Vector3[] _deathmatchSpawnPoints = new Vector3[6] {
    new Vector3(18.5182f, 0.0371f, 3.7177f),
    new Vector3(18.5824f, 0.0371f, 40.6872f),
    new Vector3(0.0182f, 0.0374f, 0.8058f),
    new Vector3(-18.5055f, 0.0371f, 40.3152f),
    new Vector3(-18.5734f, 0.0371f, 3.203f),
    new Vector3(-0.14f, 0.0371f, 21.9039f),
        };

        protected override void MainSceneInitialized() {
            // Check if this is the right map
            if (FusionSceneManager.Title == "HoloChamber" && FusionSceneManager.Level.Pallet.Internal) {
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
