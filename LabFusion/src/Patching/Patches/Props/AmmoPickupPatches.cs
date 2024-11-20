using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;

using UnityEngine;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(AmmoPickup))]
public static class AmmoPickupPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AmmoPickup.OnTriggerEnter))]
    public static bool OnTriggerEnter(Collider other)
    {
        // Make sure the ammo pickups are only triggered by ourselves and no one else
        if (NetworkInfo.HasServer && other.attachedRigidbody != null)
        {
            var rigManager = other.GetComponentInParent<RigManager>();

            if (rigManager != null && !rigManager.IsLocalPlayer())
            {
                return false;
            }
        }

        return true;
    }
}