using HarmonyLib;

using LabFusion.Scene;
using LabFusion.Utilities;

using UnityEngine;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(AmmoPickup))]
public static class AmmoPickupPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AmmoPickup.OnTriggerEnter))]
    public static bool OnTriggerEnter(Collider other)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        // Make sure the ammo pickups are only triggered by ourselves and no one else
        if (other.attachedRigidbody != null)
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