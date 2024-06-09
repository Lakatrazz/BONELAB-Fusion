using LabFusion.Utilities;
using Il2CppPuppetMasta;

namespace LabFusion.Syncables
{
    public class BehaviourPowerLegsExtender : PropComponentExtender<BehaviourPowerLegs>
    {
        public static FusionComponentCache<BehaviourPowerLegs, PropSyncable> Cache = new FusionComponentCache<BehaviourPowerLegs, PropSyncable>();

        protected override void AddToCache(BehaviourPowerLegs legs, PropSyncable syncable)
        {
            Cache.Add(legs, syncable);
        }

        protected override void RemoveFromCache(BehaviourPowerLegs legs)
        {
            Cache.Remove(legs);
        }
    }
}
