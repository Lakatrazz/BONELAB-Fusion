using LabFusion.Utilities;
using SLZ.Bonelab;

namespace LabFusion.Syncables
{
    public class PowerableJointExtender : PropComponentExtender<Powerable_Joint>
    {
        public static FusionComponentCache<Powerable_Joint, PropSyncable> Cache = new FusionComponentCache<Powerable_Joint, PropSyncable>();

        protected override void AddToCache(Powerable_Joint joint, PropSyncable syncable)
        {
            Cache.Add(joint, syncable);
        }

        protected override void RemoveFromCache(Powerable_Joint joint)
        {
            Cache.Remove(joint);
        }
    }
}
