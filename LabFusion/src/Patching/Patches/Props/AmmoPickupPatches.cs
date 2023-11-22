using HarmonyLib;
using LabFusion.Network;
using LabFusion.Utilities;
using SLZ.Bonelab;
using SLZ.Rig;
using UnityEngine;

namespace LabFusion.Patching
{
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
                var triggerRef = PlayerTriggerProxy.Cache.Get(other.attachedRigidbody.gameObject);
                if (triggerRef != null && !triggerRef.physicsRig.manager.IsSelf())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
