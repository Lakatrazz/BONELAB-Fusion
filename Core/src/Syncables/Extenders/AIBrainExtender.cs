using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;
using PuppetMasta;
using SLZ.AI;

namespace LabFusion.Syncables {
    public class AIBrainExtender : PropComponentExtender<AIBrain> {
        public static FusionComponentCache<AIBrain, PropSyncable> Cache = new FusionComponentCache<AIBrain, PropSyncable>();

        protected override void AddToCache(AIBrain brain, PropSyncable syncable) {
            Cache.Add(brain, syncable);
        }

        protected override void RemoveFromCache(AIBrain brain) {
            Cache.Remove(brain);
        }
    }
}
