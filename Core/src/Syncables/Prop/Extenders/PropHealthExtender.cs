using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Props;

namespace LabFusion.Syncables {
    public class PropHealthExtender : PropComponentArrayExtender<Prop_Health> {
        public static FusionComponentCache<Prop_Health, PropSyncable> Cache = new FusionComponentCache<Prop_Health, PropSyncable>();

        protected override void AddToCache(Prop_Health health, PropSyncable syncable) {
            Cache.Add(health, syncable);
        }

        protected override void RemoveFromCache(Prop_Health health) {
            Cache.Remove(health);
        }
    }
}
