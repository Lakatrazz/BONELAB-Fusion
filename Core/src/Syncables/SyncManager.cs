using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Extensions;
using LabFusion.Utilities;

namespace LabFusion.Syncables {
    public static class SyncManager {
        public static readonly Dictionary<ushort, ISyncable> Syncables = new Dictionary<ushort, ISyncable>(new SyncableComparer());

        public static readonly Dictionary<ushort, ISyncable> QueuedSyncables = new Dictionary<ushort, ISyncable>(new SyncableComparer());

        /// <summary>
        /// The last allocated id. Incremented server side.
        /// </summary>
        public static ushort LastId = 0;

        /// <summary>
        /// The last registered queue id. Only kept client side.
        /// </summary>
        public static ushort LastQueueId = 0;

        public static void OnCleanup() {
            foreach (var syncable in Syncables.Values)
                syncable.Cleanup();

            Syncables.Clear();

            foreach (var syncable in QueuedSyncables)
                syncable.Value.Cleanup();

            QueuedSyncables.Clear();

            LastId = 0;
            LastQueueId = 0;
        }

        public static ushort AllocateSyncID() {
            LastId++;
            return LastId;
        }

        public static ushort AllocateQueueID() {
            LastQueueId++;
            return LastQueueId;
        }

        public static void RegisterSyncable(ISyncable syncable, ushort id) {
            RemoveSyncable(id);

            syncable.OnRegister(id);
            Syncables.Add(id, syncable);
            LastId = id;
        }

        public static void RemoveSyncable(ISyncable syncable) {
            if (Syncables.ContainsValue(syncable))
                Syncables.Remove(syncable.GetId());

            if (QueuedSyncables.ContainsValue(syncable))
                QueuedSyncables.Remove(syncable.GetId());

            syncable.Cleanup();
        }

        public static void RemoveSyncable(ushort id) {
            if (Syncables.ContainsKey(id)) {
                var syncToRemove = Syncables[id];
                Syncables.Remove(id);
                syncToRemove.Cleanup();
            }
        }

        public static ushort QueueSyncable(ISyncable syncable) {
            if (QueuedSyncables.ContainsValue(syncable)) {
                var pair = QueuedSyncables.First(o => o.Value == syncable);
                QueuedSyncables.Remove(pair.Key);
            }

            var id = AllocateQueueID();
            QueuedSyncables.Add(id, syncable);
            return id;
        }

        public static bool UnqueueSyncable(ushort queuedId, ushort newId, out ISyncable syncable) {
            syncable = null;

            if (QueuedSyncables.ContainsKey(queuedId)) {
                syncable = QueuedSyncables[queuedId];
                QueuedSyncables.Remove(queuedId);
                RegisterSyncable(syncable, newId);

                return true;
            }

            return false;
        }

        public static bool TryGetSyncable(ushort id, out ISyncable syncable) => Syncables.TryGetValue(id, out syncable);
    }
}
