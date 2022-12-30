using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Props.Weapons;

namespace LabFusion.Syncables {
    public class GunExtender : PropComponentExtender<Gun> {
        public static FusionComponentCache<Gun, PropSyncable> Cache = new FusionComponentCache<Gun, PropSyncable>();

        protected override void AddToCache(Gun gun, PropSyncable syncable) {
            Cache.Add(gun, syncable);
        }

        protected override void RemoveFromCache(Gun gun) {
            Cache.Remove(gun);
        }
    }
}
