using LabFusion.Extensions;
using LabFusion.MonoBehaviours;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Bonelab;

using UnityEngine;

namespace LabFusion.Syncables
{
    public class FlyingGunExtender : PropComponentExtender<FlyingGun>
    {
        public static FusionComponentCache<FlyingGun, PropSyncable> Cache = new FusionComponentCache<FlyingGun, PropSyncable>();

        public TimedDespawner Despawner;

        protected override void AddToCache(FlyingGun gun, PropSyncable syncable)
        {
            Cache.Add(gun, syncable);
            Despawner = gun.gameObject.AddComponent<TimedDespawner>();
            Despawner.syncable = syncable;
        }

        protected override void RemoveFromCache(FlyingGun gun)
        {
            Cache.Remove(gun);

            if (!Despawner.IsNOC())
                GameObject.Destroy(Despawner);
        }

        public override void OnHeld()
        {
            Despawner.Refresh();
        }

        public override void OnAttach(Hand hand, Grip grip)
        {
            var rm = hand.manager;

            if (NetworkInfo.IsServer && PlayerRepManager.TryGetPlayerRep(rm, out var rep) && FusionDevTools.DespawnDevTool(rep.PlayerId))
            {
                if (PropSyncable.Poolee != null)
                    PropSyncable.Poolee.Despawn();
            }
        }


        public override void OnUpdate()
        {
            if (PropSyncable.IsMissingRigidbodies())
                Despawner.Refresh();
        }
    }
}
