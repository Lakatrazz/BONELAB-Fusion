using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Scene;

using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.AI;

using UnityEngine;

using HarmonyLib;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(ImpactProperties))]
public static class ImpactPropertiesPatches
{
    [HarmonyPatch(nameof(ImpactProperties.ReceiveAttack))]
    [HarmonyPrefix]
    public static void ReceiveAttack(Attack attack)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        OnProcessAttack(attack);
    }

    private static void OnProcessAttack(Attack attack)
    {
        Collider collider = attack.collider;
        TriggerRefProxy proxy = attack.proxy;

        // Check if this was a bullet attack + it was us who shot the bullet
        if (proxy == RigData.Refs.Proxy && attack.attackType == AttackType.Piercing)
        {
            var rb = collider.attachedRigidbody;

            if (rb != null)
            {
                ImpactUtilities.OnHitRigidbody(rb);
            }
        }
    }
}
