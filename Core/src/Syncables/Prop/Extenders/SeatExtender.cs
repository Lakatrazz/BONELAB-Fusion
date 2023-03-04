using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Vehicle;

namespace LabFusion.Syncables {
    public class SeatExtender : PropComponentArrayExtender<Seat> {
        public static FusionComponentCache<Seat, PropSyncable> Cache = new FusionComponentCache<Seat, PropSyncable>();

        protected override void AddToCache(Seat seat, PropSyncable syncable) {
            Cache.Add(seat, syncable);

            syncable.IsVehicle = true;
        }

        protected override void RemoveFromCache(Seat seat) {
            Cache.Remove(seat);
        }
    }
}
