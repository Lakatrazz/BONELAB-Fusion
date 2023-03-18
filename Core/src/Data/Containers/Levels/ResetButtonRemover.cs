using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class ResetButtonRemover : LevelDataHandler {
        protected override void MainSceneInitialized() {
            // Loop through all gameObjects in the scene and remove hub buttons
            if (NetworkInfo.HasServer) {
                // Get all scene gameobjects
                var gameObjects = GameObject.FindObjectsOfType<GameObject>();

                for (var i = 0; i < gameObjects.Length; i++)
                {
                    var go = gameObjects[i];

                    // Get name
                    string name = go.name;

                    // Reload scene/hub buttons
                    if (name.Contains("prop_bigButton_LOADHUB") || name.Contains("prop_bigButton_RESET"))
                        go.SetActive(false);
                }
            }
        }
    }
}
