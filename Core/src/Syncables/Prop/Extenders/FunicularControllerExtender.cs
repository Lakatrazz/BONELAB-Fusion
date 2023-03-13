using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Bonelab;

namespace LabFusion.Syncables {
    public class FunicularControllerExtender : PropComponentExtender<FunicularController> {
        public static FusionComponentCache<FunicularController, PropSyncable> Cache = new FusionComponentCache<FunicularController, PropSyncable>();

        protected override void AddToCache(FunicularController funicular, PropSyncable syncable) {
            Cache.Add(funicular, syncable);
        }

        protected override void RemoveFromCache(FunicularController funicular) {
            Cache.Remove(funicular);
        }
    }
}
