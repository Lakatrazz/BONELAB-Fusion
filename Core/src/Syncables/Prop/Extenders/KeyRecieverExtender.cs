using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using SLZ.Interaction;
using SLZ.Props.Weapons;

namespace LabFusion.Syncables {
    public class KeyRecieverExtender : PropComponentArrayExtender<KeyReciever> {
        public static FusionComponentCache<KeyReciever, PropSyncable> Cache = new FusionComponentCache<KeyReciever, PropSyncable>();

        protected override void AddToCache(KeyReciever receiver, PropSyncable syncable) {
            Cache.Add(receiver, syncable);
        }

        protected override void RemoveFromCache(KeyReciever receiver) {
            Cache.Remove(receiver);
        }
    }
}
