using HarmonyLib;
using LabFusion.Network;
using SLZ.Bonelab;
using SLZ.Marrow.Pool;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(SpawnEvents))]
    public static class SpawnEventsPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpawnEvents.Despawn))]
        public static void DespawnPrefix(SpawnEvents __instance, ref bool __state)
        {
            if (NetworkInfo.HasServer && __instance.TryCast<AmmoPickup>())
            {
                PooleeDespawnPatch.IgnorePatch = true;
                __state = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpawnEvents.Despawn))]
        public static void DespawnPostfix(SpawnEvents __instance, bool __state)
        {
            if (__state)
            {
                PooleeDespawnPatch.IgnorePatch = false;
            }
        }
    }
}
