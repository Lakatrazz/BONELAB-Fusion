using Il2CppSLZ.Bonelab;
using LabFusion.Utilities;

namespace LabFusion.Syncables
{
    public class SimpleGripEventsExtender : PropComponentArrayExtender<SimpleGripEvents>
    {
        public static FusionComponentCache<SimpleGripEvents, PropSyncable> Cache = new FusionComponentCache<SimpleGripEvents, PropSyncable>();

        protected override void AddToCache(SimpleGripEvents events, PropSyncable syncable)
        {
            Cache.Add(events, syncable);
        }

        protected override void RemoveFromCache(SimpleGripEvents events)
        {
            Cache.Remove(events);
        }
    }
}
