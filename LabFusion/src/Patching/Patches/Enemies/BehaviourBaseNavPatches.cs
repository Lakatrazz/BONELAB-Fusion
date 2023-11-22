using HarmonyLib;
using PuppetMasta;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(BehaviourBaseNav))]
    public static class BehaviourBaseNavPatches
    {
        public static bool IgnorePatches = false;
    }
}
