using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Bonelab;

namespace LabFusion.Syncables {
    public class TwoButtonRemoteControllerExtender : PropComponentExtender<TwoButtonRemoteController> {
        public static FusionComponentCache<TwoButtonRemoteController, PropSyncable> Cache = new FusionComponentCache<TwoButtonRemoteController, PropSyncable>();

        protected override void AddToCache(TwoButtonRemoteController controller, PropSyncable syncable) {
            Cache.Add(controller, syncable);
        }

        protected override void RemoveFromCache(TwoButtonRemoteController controller) {
            Cache.Remove(controller);
        }
    }
}
