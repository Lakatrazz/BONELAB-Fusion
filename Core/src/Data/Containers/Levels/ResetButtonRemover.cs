using LabFusion.Network;

using SLZ.Interaction;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public sealed class ResetButtonRemover : LevelDataHandler {
        // This should always apply to all levels.
        public override string LevelTitle => null;

        // List of all blacklisted names
        private static readonly string[] _blacklistedButtons = new string[] {
            "prop_bigButton_LOADHUB",
            "prop_bigButton_RESET",
            "prop_bigButton_NEXTLEVEL",
        };

        protected override void MainSceneInitialized() {
            // Loop through all buttons in the scene and disable hub buttons
            if (NetworkInfo.HasServer) {
                // Get all buttons
                var buttons = GameObject.FindObjectsOfType<ButtonToggle>();

                for (var i = 0; i < buttons.Length; i++)
                {
                    var button = buttons[i];

                    // Get name
                    string name = button.name;
                    
                    // Check if the name is blacklisted
                    foreach (var blacklist in _blacklistedButtons) {
                        if (name.Contains(blacklist)) {
                            button.enabled = false;
                            break;
                        }
                    }
                }
            }
        }
    }
}
