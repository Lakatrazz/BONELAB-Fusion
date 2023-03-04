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

using SLZ.Interaction;
using SLZ.Props;

using UnityEngine;

namespace LabFusion.Syncables {
    public class SpawnGunExtender : PropComponentExtender<SpawnGun> {
        public static FusionComponentCache<SpawnGun, PropSyncable> Cache = new FusionComponentCache<SpawnGun, PropSyncable>();

        public TimedDespawner Despawner;

        protected override void AddToCache(SpawnGun gun, PropSyncable syncable) {
            Cache.Add(gun, syncable);
            Despawner = gun.gameObject.AddComponent<TimedDespawner>();
            Despawner.syncable = syncable;
        }

        protected override void RemoveFromCache(SpawnGun gun) {
            Cache.Remove(gun);

            if (!Despawner.IsNOC())
                GameObject.Destroy(Despawner);
        }

        public override void OnHeld() {
            Despawner.Refresh();
        }

        public override void OnAttach(Hand hand, Grip grip) {
            var rm = hand.manager;

            if (NetworkInfo.IsServer && PlayerRepManager.TryGetPlayerRep(rm, out var rep) && FusionDevTools.DespawnDevTool(rep.PlayerId)) {
                if (PropSyncable.AssetPoolee != null)
                    PropSyncable.AssetPoolee.Despawn();
            }
        }

        public override void OnUpdate()
        {
            if (PropSyncable.IsMissingRigidbodies())
                Despawner.Refresh();
        }
    }
}
