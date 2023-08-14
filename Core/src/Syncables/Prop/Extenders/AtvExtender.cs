using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Vehicle;

using UnityEngine;

namespace LabFusion.Syncables {
    public class AtvExtender : PropComponentExtender<Atv>, IOwnerLocker {
        public static FusionComponentCache<Atv, PropSyncable> Cache = new FusionComponentCache<Atv, PropSyncable>();

        protected override void AddToCache(Atv atv, PropSyncable syncable) {
            Cache.Add(atv, syncable);
            syncable.AddOwnerLocker(this);
        }

        protected override void RemoveFromCache(Atv atv) {
            Cache.Remove(atv);
        }

        public bool CheckLock(out byte owner) {
            owner = 0;
            
            // Check if there's a player in the seat
            var seat = Component.driverSeat;
            if (seat != null && seat.rigManager != null) {
                // Get owner id
                var rm = seat.rigManager;

                if (rm.IsSelf()) {
                    owner = PlayerIdManager.LocalSmallId;
                    return true;
                }
                else if (PlayerRepManager.TryGetPlayerRep(rm, out var rep)) {
                    owner = rep.PlayerId;
                    return true;
                }
            }

            return false;
        }
    }
}
