using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Props;
using SLZ.Props.Weapons;

namespace LabFusion.Syncables {
    public class PropFlashlightExtender : PropComponentExtender<PropFlashlight> {
        public static FusionComponentCache<PropFlashlight, PropSyncable> Cache = new FusionComponentCache<PropFlashlight, PropSyncable>();

        protected override void AddToCache(PropFlashlight flashlight, PropSyncable syncable) {
            Cache.Add(flashlight, syncable);
        }

        protected override void RemoveFromCache(PropFlashlight flashlight) {
            Cache.Remove(flashlight);
        }
    }
}
