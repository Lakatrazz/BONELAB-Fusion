using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

namespace LabFusion.Syncables
{
    public class AmmoSocketExtender : PropComponentExtender<AmmoSocket>
    {
        public static FusionComponentCache<AmmoSocket, PropSyncable> Cache = new FusionComponentCache<AmmoSocket, PropSyncable>();

        protected override void AddToCache(AmmoSocket socket, PropSyncable syncable)
        {
            Cache.Add(socket, syncable);
        }

        protected override void RemoveFromCache(AmmoSocket socket)
        {
            Cache.Remove(socket);
        }
    }
}
