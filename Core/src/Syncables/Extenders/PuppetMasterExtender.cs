using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using PuppetMasta;

namespace LabFusion.Syncables {
    public class PuppetMasterExtender : PropComponentExtender<PuppetMaster> {
        public static FusionComponentCache<PuppetMaster, PropSyncable> Cache = new FusionComponentCache<PuppetMaster, PropSyncable>();

        protected override void AddToCache(PuppetMaster puppet, PropSyncable syncable) {
            Cache.Add(puppet, syncable);
        }

        protected override void RemoveFromCache(PuppetMaster puppet) {
            Cache.Remove(puppet);
        }
    }
}
