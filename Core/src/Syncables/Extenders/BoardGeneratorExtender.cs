using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Bonelab;
using SLZ.Interaction;
using SLZ.Props;
using SLZ.Props.Weapons;

namespace LabFusion.Syncables {
    public class BoardGeneratorExtender : PropComponentExtender<BoardGenerator> {
        public static FusionComponentCache<BoardGenerator, PropSyncable> Cache = new FusionComponentCache<BoardGenerator, PropSyncable>();

        protected override void AddToCache(BoardGenerator generator, PropSyncable syncable) {
            Cache.Add(generator, syncable);
        }

        protected override void RemoveFromCache(BoardGenerator generator) {
            Cache.Remove(generator);
        }
    }
}
