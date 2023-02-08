using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Bonelab;
using SLZ.Props.Weapons;

namespace LabFusion.Syncables {
    public class EscalatorExtender : PropComponentExtender<Escalator> {
        public static FusionComponentCache<Escalator, PropSyncable> Cache = new FusionComponentCache<Escalator, PropSyncable>();

        protected override void AddToCache(Escalator escalator, PropSyncable syncable) {
            Cache.Add(escalator, syncable);

            // Don't sync escalators due to their weirdness
            syncable.DisableSyncing = true;
        }

        protected override void RemoveFromCache(Escalator escalator) {
            Cache.Remove(escalator);
        }
    }
}
