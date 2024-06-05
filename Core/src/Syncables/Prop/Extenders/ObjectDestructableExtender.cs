using LabFusion.Utilities;

using SLZ.Props;

namespace LabFusion.Syncables
{
    public class ObjectDestructableExtender : PropComponentArrayExtender<ObjectDestructable>
    {
        public static FusionComponentCache<ObjectDestructable, PropSyncable> Cache = new FusionComponentCache<ObjectDestructable, PropSyncable>();

        protected override void AddToCache(ObjectDestructable destructable, PropSyncable syncable)
        {
            Cache.Add(destructable, syncable);
        }

        protected override void RemoveFromCache(ObjectDestructable destructable)
        {
            Cache.Remove(destructable);
        }
    }
}
