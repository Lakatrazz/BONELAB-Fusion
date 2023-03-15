using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using SLZ.AI;
using SLZ.Bonelab;
using SLZ.Rig;
using UnityEngine;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(AmmoPickup))]
    public static class AmmoPickupPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(AmmoPickup.OnTriggerEnter))]
        public static bool OnTriggerEnter(Collider other) {
            // Make sure the ammo pickups are only triggered by ourselves and no one else
            if (NetworkInfo.HasServer && other.attachedRigidbody != null) {
                var triggerRef = PlayerTriggerProxy.Cache.Get(other.attachedRigidbody.gameObject);
                if (triggerRef != null && !triggerRef.physicsRig.manager.IsLocalPlayer()) {
                    return false;
                }
            }
            
            return true;
        }
    }
}
