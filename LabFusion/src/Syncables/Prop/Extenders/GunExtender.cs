using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Syncables
{
    public class GunExtender : PropComponentArrayExtender<Gun>
    {
        public static FusionComponentCache<Gun, PropSyncable> Cache = new FusionComponentCache<Gun, PropSyncable>();

        protected override void AddToCache(Gun gun, PropSyncable syncable)
        {
            Cache.Add(gun, syncable);
        }

        protected override void RemoveFromCache(Gun gun)
        {
            Cache.Remove(gun);
        }
    }
}
