using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.SceneManagement;
using UnityEngine.Events;
using BoneLib;

using LabFusion.Patching;

namespace LabFusion.Utilities {
    internal static class PatchingUtilities {
        internal static void PatchAll() {
            // Patch native methods
            VirtualControllerPatches.Patch();
            SubBehaviourHealthPatches.Patch();
            ImpactPropertiesPatches.Patch();
            PlayerDamageReceiverPatches.Patch();

            // Run manual patches if on PC
            if (HelperMethods.IsAndroid()) {
                FusionLogger.Warn("Skipping specific harmony patches! " +
                    "Please note that these are important for some functions of the mod and need to be fixed in the future!");
            }
            else {
                Internal_PatchManuals();
            }
        }

        private static void Internal_PatchManuals() {
            AssetPooleePatches.Patch();
            ChunkTriggerPatches.Patch();
        }
    }
}
