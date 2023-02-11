using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Extensions;
using LabFusion.MonoBehaviours;
using LabFusion.Utilities;

using SLZ.Props;

using UnityEngine;

namespace LabFusion.Syncables {
    public class FlyingGunExtender : PropComponentExtender<FlyingGun> {
        public static FusionComponentCache<FlyingGun, PropSyncable> Cache = new FusionComponentCache<FlyingGun, PropSyncable>();

        public TimedDespawner Despawner;

        protected override void AddToCache(FlyingGun gun, PropSyncable syncable) {
            Cache.Add(gun, syncable);
            Despawner = gun.gameObject.AddComponent<TimedDespawner>();
        }

        protected override void RemoveFromCache(FlyingGun gun) {
            Cache.Remove(gun);

            if (!Despawner.IsNOC())
                GameObject.Destroy(Despawner);
        }

        public override void OnHeld() {
            Despawner.Refresh();
        }

        public override void OnUpdate() {
            if (PropSyncable.IsMissingRigidbodies())
                Despawner.Refresh();
        }
    }
}
