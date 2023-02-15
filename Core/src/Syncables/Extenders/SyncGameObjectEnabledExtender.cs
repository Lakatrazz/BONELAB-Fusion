using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

namespace LabFusion.Syncables {
    public class SyncGameObjectEnabledExtender : PropComponentArrayExtender<SyncGameObjectEnabled> {
        public static FusionComponentCache<SyncGameObjectEnabled, PropSyncable> Cache = new FusionComponentCache<SyncGameObjectEnabled, PropSyncable>();

        protected override void AddToCache(SyncGameObjectEnabled script, PropSyncable syncable) {
            Cache.Add(script, syncable);

            script.PropSyncable = syncable;
        }

        protected override void RemoveFromCache(SyncGameObjectEnabled script) {
            Cache.Remove(script);

            script.PropSyncable = null;
        }
    }
}
