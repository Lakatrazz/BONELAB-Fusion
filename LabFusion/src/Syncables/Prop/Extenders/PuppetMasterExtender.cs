using LabFusion.Utilities;

using Il2CppSLZ.Marrow.PuppetMasta;

namespace LabFusion.Syncables
{
    public class PuppetMasterExtender : PropComponentExtender<PuppetMaster>
    {
        public static FusionComponentCache<PuppetMaster, PropSyncable> Cache = new();

        public static PropSyncable LastKilled = null;

        protected override void AddToCache(PuppetMaster puppet, PropSyncable syncable)
        {
            Cache.Add(puppet, syncable);
        }

        protected override void RemoveFromCache(PuppetMaster puppet)
        {
            Cache.Remove(puppet);
        }

        public override void OnOwnershipTransfer()
        {
            // Force update slerp drives
            bool isOwner = PropSyncable.IsOwner();

            float muscleWeightMaster = Component.muscleWeight;
            float muscleSpring = Component.muscleSpring;
            float muscleDamper = Component.muscleDamper;

            foreach (var muscle in Component.muscles)
            {
                if (isOwner)
                {
                    muscle.MusclePdDrive(muscleWeightMaster, muscleSpring, muscleDamper);
                }
                else
                {
                    muscle.MusclePdDrive(0f, 0f, 0f);
                }
            }
        }
    }
}
