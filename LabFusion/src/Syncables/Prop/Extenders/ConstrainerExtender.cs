using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Bonelab;

namespace LabFusion.Syncables
{
    public class ConstrainerExtender : PropComponentExtender<Constrainer>
    {
        public static FusionComponentCache<Constrainer, PropSyncable> Cache = new FusionComponentCache<Constrainer, PropSyncable>();

        protected override void AddToCache(Constrainer constrainer, PropSyncable syncable)
        {
            Cache.Add(constrainer, syncable);
        }

        protected override void RemoveFromCache(Constrainer constrainer)
        {
            Cache.Remove(constrainer);
        }

        public override void OnAttach(Hand hand, Grip grip)
        {
            var rm = hand.manager;

            if (NetworkInfo.IsServer && PlayerRepManager.TryGetPlayerRep(rm, out var rep) && FusionDevTools.DespawnConstrainer(rep.PlayerId))
            {
                if (PropSyncable.Poolee != null)
                    PropSyncable.Poolee.Despawn();
            }
        }
    }
}
