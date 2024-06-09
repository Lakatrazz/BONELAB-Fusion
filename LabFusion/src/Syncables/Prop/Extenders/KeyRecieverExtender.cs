using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

namespace LabFusion.Syncables
{
    public class KeyRecieverExtender : PropComponentArrayExtender<KeyReceiver>
    {
        public static FusionComponentCache<KeyReceiver, PropSyncable> Cache = new FusionComponentCache<KeyReceiver, PropSyncable>();

        protected override void AddToCache(KeyReceiver receiver, PropSyncable syncable)
        {
            Cache.Add(receiver, syncable);
        }

        protected override void RemoveFromCache(KeyReceiver receiver)
        {
            Cache.Remove(receiver);
        }
    }
}
