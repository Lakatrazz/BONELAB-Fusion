using LabFusion.Utilities;

using Il2CppSLZ.VFX;

namespace LabFusion.Syncables
{
    public class ObjectDestructableExtender : PropComponentArrayExtender<ObjectDestructible>
    {
        public static FusionComponentCache<ObjectDestructible, PropSyncable> Cache = new FusionComponentCache<ObjectDestructible, PropSyncable>();

        protected override void AddToCache(ObjectDestructible destructable, PropSyncable syncable)
        {
            Cache.Add(destructable, syncable);
        }

        protected override void RemoveFromCache(ObjectDestructible destructable)
        {
            Cache.Remove(destructable);
        }
    }
}
