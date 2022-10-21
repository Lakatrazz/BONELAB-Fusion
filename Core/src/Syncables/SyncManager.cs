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
        public static readonly Dictionary<ushort, ISyncable> Syncables = new Dictionary<ushort, ISyncable>();

        public static readonly List<ISyncable> QueuedSyncables = new List<ISyncable>();

        public static ushort LastId = 0;

        public static void OnCleanup() {
            foreach (var syncable in Syncables.Values)
                syncable.Cleanup();

            Syncables.Clear();

            foreach (var syncable in QueuedSyncables)
                syncable.Cleanup();

            QueuedSyncables.Clear();

            LastId = 0;
        }

        public static ushort AllocateSyncID() {
            LastId++;
            return LastId;
        }

        public static void RegisterSyncable(ISyncable syncable, ushort id) {
            if (Syncables.ContainsKey(id)) {
                Syncables.Remove(id);
            }

            syncable.OnRegister(id);
            Syncables.Add(id, syncable);
            LastId = id;
        }

        public static void RemoveSyncable(ISyncable syncable) {
            if (Syncables.ContainsValue(syncable))
                Syncables.Remove(syncable.GetId());

            if (QueuedSyncables.Contains(syncable))
                QueuedSyncables.Remove(syncable);

            syncable.Cleanup();
        }

        public static ushort QueueSyncable(ISyncable syncable) {
            if (QueuedSyncables.Contains(syncable)) {
                int index = QueuedSyncables.FindIndex(o => o == syncable);
                QueuedSyncables.RemoveAt(index);
            }
            
            QueuedSyncables.Add(syncable);
            return (ushort)QueuedSyncables.IndexOf(syncable);
        }

        public static bool UnqueueSyncable(ushort queuedId, ushort newId, out ISyncable syncable) {
            syncable = null;

            if (QueuedSyncables.Count > queuedId) {
                syncable = QueuedSyncables[queuedId];
                QueuedSyncables.RemoveAt(queuedId);
                RegisterSyncable(syncable, newId);

                return true;
            }

            return false;
        }

        public static bool TryGetSyncable(ushort id, out ISyncable syncable) => Syncables.TryGetValue(id, out syncable);
    }
}
