using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Patching
{
    internal class ManualPatcher
    {
        private static bool hasPatched = false;
        public static void PatchAll(HarmonyLib.Harmony harmonyInstance)
        {
            if (hasPatched)
                return;

            VirtualControllerPatches.Patch(harmonyInstance);

            hasPatched = true;
        }
    }
}
