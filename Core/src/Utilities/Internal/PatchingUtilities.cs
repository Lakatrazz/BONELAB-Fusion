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

            // Run our manual patches
            Internal_PatchManuals();
        }

        private static void Internal_PatchManuals() {
            // These patches used to break on android, but trev figured out how to make them work
            AssetPooleePatches.Patch();
            ChunkTriggerPatches.Patch();
        }
    }
}
