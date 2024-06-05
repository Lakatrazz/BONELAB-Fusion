using LabFusion.Extensions;
using LabFusion.Utilities;

using SLZ.Interaction;
using SLZ.Props;

namespace LabFusion.Syncables
{
    public class ConstraintSyncable : Syncable
    {
        public static FusionComponentCache<ConstraintTracker, ConstraintSyncable> Cache = new FusionComponentCache<ConstraintTracker, ConstraintSyncable>();

        public readonly ConstraintTracker Tracker;

        public ConstraintSyncable(ConstraintTracker tracker)
        {
            Tracker = tracker;
            Cache.Add(tracker, this);
        }

        public override void Cleanup()
        {
            if (!Tracker.IsNOC())
                Cache.Remove(Tracker);

            base.Cleanup();
        }

        public override Grip GetGrip(ushort index) => null;

        public override ushort? GetIndex(Grip grip) => null;

        public override byte? GetOwner() => null;

        public override bool IsGrabbed() => false;

        public override bool IsOwner() => false;

        public override void OnFixedUpdate() { }

        public override void OnUpdate() { }

        public override void SetOwner(byte owner) { }

        public override void RemoveOwner() { }
    }
}
