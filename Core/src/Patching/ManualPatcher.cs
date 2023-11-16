using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Patching
{
    internal static class ManualPatcher
    {
        internal static void PatchAll()
        {
            // Native patches
            // Patched due to structs or other IL2CPP issues
            VirtualControllerPatches.Patch();
            SubBehaviourHealthPatches.Patch();
            ImpactPropertiesPatches.Patch();
            PlayerDamageReceiverPatches.Patch();
        }
    }
}
