using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Interaction;
using SLZ.Props.Weapons;

namespace LabFusion.Syncables {
    public class GripExtender : PropComponentArrayExtender<Grip> {
        public static FusionComponentCache<Grip, PropSyncable> Cache = new FusionComponentCache<Grip, PropSyncable>();

        protected override void AddToCache(Grip grip, PropSyncable syncable) {
            Cache.Add(grip, syncable);
        }

        protected override void RemoveFromCache(Grip grip) {
            Cache.Remove(grip);
        }
    }
}
