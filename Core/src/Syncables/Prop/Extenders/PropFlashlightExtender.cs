using LabFusion.Utilities;

using SLZ.Props;

namespace LabFusion.Syncables
{
    public class PropFlashlightExtender : PropComponentExtender<PropFlashlight>
    {
        public static FusionComponentCache<PropFlashlight, PropSyncable> Cache = new FusionComponentCache<PropFlashlight, PropSyncable>();

        protected override void AddToCache(PropFlashlight flashlight, PropSyncable syncable)
        {
            Cache.Add(flashlight, syncable);
        }

        protected override void RemoveFromCache(PropFlashlight flashlight)
        {
            Cache.Remove(flashlight);
        }
    }
}
