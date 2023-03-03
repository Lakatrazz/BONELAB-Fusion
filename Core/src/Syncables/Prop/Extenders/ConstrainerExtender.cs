using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Interaction;
using SLZ.Props;
using SLZ.Props.Weapons;

namespace LabFusion.Syncables {
    public class ConstrainerExtender : PropComponentExtender<Constrainer> {
        public static FusionComponentCache<Constrainer, PropSyncable> Cache = new FusionComponentCache<Constrainer, PropSyncable>();

        protected override void AddToCache(Constrainer constrainer, PropSyncable syncable) {
            Cache.Add(constrainer, syncable);
        }

        protected override void RemoveFromCache(Constrainer constrainer) {
            Cache.Remove(constrainer);
        }
    }
}
