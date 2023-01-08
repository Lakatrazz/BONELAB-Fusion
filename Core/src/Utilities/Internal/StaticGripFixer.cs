using SLZ.Bonelab;
using SLZ.Interaction;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    internal static class StaticGripFixer {
        internal static void OnMainSceneInitialized() {
            // Ammo dispenser
            var ammoDispensers = GameObject.FindObjectsOfType<AmmoDispenser>();

            foreach (var dispenser in ammoDispensers) {
                dispenser.gameObject.AddComponent<InteractableHost>();
            }
        }
    }
}
