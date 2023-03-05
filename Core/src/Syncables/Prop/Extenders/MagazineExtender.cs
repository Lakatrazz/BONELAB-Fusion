using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Extensions;
using LabFusion.MonoBehaviours;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Props.Weapons;

using UnityEngine;

namespace LabFusion.Syncables {
    public class MagazineExtender : PropComponentExtender<Magazine> {
        public static FusionComponentCache<Magazine, PropSyncable> Cache = new FusionComponentCache<Magazine, PropSyncable>();

        public TimedDespawner Despawner;

        protected override void AddToCache(Magazine mag, PropSyncable syncable) {
            Cache.Add(mag, syncable);
            Despawner = mag.gameObject.AddComponent<TimedDespawner>();
            Despawner.syncable = syncable;
            Despawner.totalTime = 20f;
        }

        protected override void RemoveFromCache(Magazine mag) {
            Cache.Remove(mag);

            if (!Despawner.IsNOC())
                GameObject.Destroy(Despawner);
        }

        public override void OnHeld()
        {
            Despawner.Refresh();
        }

        public override void OnUpdate()
        {
            if (PropSyncable.IsMissingRigidbodies() || Component.magazinePlug._isLocked)
                Despawner.Refresh();
        }
    }
}
