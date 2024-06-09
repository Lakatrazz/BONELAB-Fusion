using LabFusion.Utilities;

using Il2CppSLZ.Marrow.AI;

namespace LabFusion.Syncables
{
    public class TriggerRefProxyExtender : PropComponentExtender<TriggerRefProxy>
    {
        public static FusionComponentCache<TriggerRefProxy, PropSyncable> Cache = new FusionComponentCache<TriggerRefProxy, PropSyncable>();

        protected override void AddToCache(TriggerRefProxy proxy, PropSyncable syncable)
        {
            Cache.Add(proxy, syncable);
        }

        protected override void RemoveFromCache(TriggerRefProxy proxy)
        {
            Cache.Remove(proxy);
        }
    }
}
