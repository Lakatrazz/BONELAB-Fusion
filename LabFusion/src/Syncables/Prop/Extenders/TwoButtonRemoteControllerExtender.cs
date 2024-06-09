using LabFusion.Utilities;

using Il2CppSLZ.Bonelab.Obsolete;

namespace LabFusion.Syncables
{
    public class TwoButtonRemoteControllerExtender : PropComponentExtender<TwoButtonRemoteController>
    {
        public static FusionComponentCache<TwoButtonRemoteController, PropSyncable> Cache = new FusionComponentCache<TwoButtonRemoteController, PropSyncable>();

        protected override void AddToCache(TwoButtonRemoteController controller, PropSyncable syncable)
        {
            Cache.Add(controller, syncable);
        }

        protected override void RemoveFromCache(TwoButtonRemoteController controller)
        {
            Cache.Remove(controller);
        }
    }
}
