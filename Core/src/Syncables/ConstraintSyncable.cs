using LabFusion.Extensions;
using LabFusion.Utilities;

using SLZ.Interaction;
using SLZ.Props;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Syncables
{
    public class ConstraintSyncable : ISyncable
    {
        public static FusionComponentCache<ConstraintTracker, ConstraintSyncable> Cache = new FusionComponentCache<ConstraintTracker, ConstraintSyncable>();

        public readonly ConstraintTracker Tracker;

        public ushort Id;

        private bool _hasRegistered = false;

        public ConstraintSyncable(ConstraintTracker tracker) {
            Tracker = tracker;
            Cache.Add(tracker, this);
        }

        public void Cleanup() {
            if (!Tracker.IsNOC())
                Cache.Remove(Tracker);
        }

        Grip ISyncable.GetGrip(ushort index) => throw new NotImplementedException();

        public ushort GetId() => Id;

        public ushort? GetIndex(Grip grip) => throw new NotImplementedException();

        byte? ISyncable.GetOwner() => throw new NotImplementedException();

        bool ISyncable.IsGrabbed() => throw new NotImplementedException();

        bool ISyncable.IsOwner() => throw new NotImplementedException();

        public bool IsQueued() {
            return SyncManager.QueuedSyncables.ContainsValue(this);
        }

        public bool IsRegistered() => _hasRegistered;

        void ISyncable.OnFixedUpdate() { }

        public void OnRegister(ushort id) {
            Id = id;
            _hasRegistered = true;
        }

        void ISyncable.OnUpdate() { }

        void ISyncable.SetOwner(byte owner) => throw new NotImplementedException();
    }
}
