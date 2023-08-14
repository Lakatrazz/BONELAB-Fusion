using LabFusion.Network;

using SLZ.Interaction;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Events;

namespace LabFusion.Data {
    public sealed class ResetButtonRemover : LevelDataHandler {
        // This should always apply to all levels.
        public override string LevelTitle => null;

        // List of all blacklisted names
        private static readonly string[] _blacklistedButtons = new string[] {
            "prop_bigButton_LOADHUB",
            "prop_bigButton_RESET",
            "prop_bigButton_LoadHub",
            "prop_bigButton (1)_01",
            "FLOORS",
            "prop_bigButton_floating_RESET",
            "prop_bigButton_floating_HUB",
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
                    string name = button.gameObject.name;
                    string parentName = button.transform.parent ? button.transform.parent.gameObject.name : name;

                    // Check if the name is blacklisted
                    foreach (var blacklist in _blacklistedButtons) {
                        if (name.Contains(blacklist) || parentName.Contains(blacklist)) {
                            button.onPress = new UnityEvent();
                            button.onDepress = new UnityEvent();
                            button.onHold = new UnityEvent();
                            button.onPressOneShot = new UnityEvent();
                            break;
                        }
                    }
                }
            }
        }
    }
}
