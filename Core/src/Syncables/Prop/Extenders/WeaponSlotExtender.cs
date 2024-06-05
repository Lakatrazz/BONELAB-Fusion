using LabFusion.Utilities;

using SLZ.Props.Weapons;

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
