using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Props;
using SLZ.Props.Weapons;

namespace LabFusion.Syncables {
    public class NimbusGunExtender : PropComponentExtender<FlyingGun> {
        public static FusionComponentCache<FlyingGun, PropSyncable> Cache = new FusionComponentCache<FlyingGun, PropSyncable>();

        protected override void AddToCache(FlyingGun gun, PropSyncable syncable) {
            Cache.Add(gun, syncable);
        }

        protected override void RemoveFromCache(FlyingGun gun) {
            Cache.Remove(gun);
        }
    }
}
