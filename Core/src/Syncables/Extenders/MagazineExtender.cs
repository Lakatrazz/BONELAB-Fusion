using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Props.Weapons;

namespace LabFusion.Syncables {
    public class MagazineExtender : PropComponentExtender<Magazine> {
        public static FusionComponentCache<Magazine, PropSyncable> Cache = new FusionComponentCache<Magazine, PropSyncable>();

        protected override void AddToCache(Magazine mag, PropSyncable syncable) {
            Cache.Add(mag, syncable);
        }

        protected override void RemoveFromCache(Magazine mag) {
            Cache.Remove(mag);
        }
    }
}
