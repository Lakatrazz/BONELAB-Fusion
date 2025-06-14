using LabFusion.Network;
using LabFusion.Marrow.Extenders;

using Il2CppSLZ.Marrow.PuppetMasta;
using Il2CppSLZ.Marrow.Combat;

using HarmonyLib;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(SubBehaviourHealth))]
public static class SubBehaviourHealthPatches
{
    [HarmonyPatch(nameof(SubBehaviourHealth.TakeDamage))]
    [HarmonyPrefix]
    public static bool TakeDamage(SubBehaviourHealth __instance, int m, Attack attack)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        if (PuppetMasterExtender.Cache.TryGet(__instance.behaviour.puppetMaster, out var entity) && !entity.IsOwner)
        {
            return false;
        }

        return true;
    }
}
