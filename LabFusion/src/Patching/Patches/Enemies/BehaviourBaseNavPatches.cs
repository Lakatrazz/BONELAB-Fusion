using HarmonyLib;

using Il2CppSLZ.Marrow.PuppetMasta;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(BehaviourBaseNav))]
    public static class BehaviourBaseNavPatches
    {
        public static bool IgnorePatches = false;
    }
}
