﻿using LabFusion.Senders;
using LabFusion.Syncables;

using MelonLoader;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    public static class ImpactUtilities {
        public static void OnHitRigidbody(Rigidbody rb) {
            var go = rb.gameObject;

            // Already has a syncable?
            if (PropSyncable.HostCache.TryGet(go, out var syncable)) {
                // Only transfer ownership if this is not currently held and not a vehicle
                if (!syncable.IsHeld && !syncable.IsVehicle)
                    PropSender.SendOwnershipTransfer(syncable);
            }
            // Create a new synced object
            else {
                // Check the blacklist
                if (!go.IsSyncWhitelisted())
                    return;

                DelayUtilities.Delay(() => { PropSender.SendPropCreation(go); }, 4);
            }
        }
    }
}
