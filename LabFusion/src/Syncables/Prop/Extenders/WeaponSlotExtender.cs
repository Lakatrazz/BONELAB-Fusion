using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

namespace LabFusion.Syncables
{
    public class WeaponSlotExtender : PropComponentExtender<WeaponSlot>
    {
        public static FusionComponentCache<WeaponSlot, PropSyncable> Cache = new FusionComponentCache<WeaponSlot, PropSyncable>();

        protected override void AddToCache(WeaponSlot slot, PropSyncable syncable)
        {
            Cache.Add(slot, syncable);
        }

        protected override void RemoveFromCache(WeaponSlot slot)
        {
            Cache.Remove(slot);
        }
    }
}
