using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Extensions;

namespace LabFusion.Utilities {
    public static class SyncUtilities {
        public enum SyncGroup : byte {
            UNKNOWN = 0,
            PLAYER_BODY = 1,
            PROP = 2,
            NPC = 3,
            STATIC = 4,
            WORLD_GRIP = 5,
        }

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

        // Handlers are created up front, they're not static
        public static void RegisterGrabTypeFromAssembly(Assembly targetAssembly)
        {
            if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

            FusionLogger.Log($"Populating GrabType list from {targetAssembly.GetName().Name}!");

            // I am aware LINQ is kinda gross but this is works!
            targetAssembly.GetTypes()
                .Where(type => typeof(SerializedGrab).IsAssignableFrom(type) && !type.IsAbstract)
                .ForEach(type => {
                    try
                    {
                        RegisterGrabType(type);
                    }
                    catch (Exception e)
                    {
                        FusionLogger.Error(e.Message);
                    }
                });
        }

        public static void RegisterGrabType<T>() where T : Type => RegisterGrabType(typeof(T));

        public static void RegisterGrabType(Type type)
        {
            var attribute = type.GetCustomAttribute(typeof(SerializedGrabGroup));
            if (attribute == null || !(attribute is SerializedGrabGroup)) {
                FusionLogger.Warn($"Didn't register {type.Name} because its grab group was null!");
            }
            else {
                SyncGroup group = ((SerializedGrabGroup)attribute).group;

                if (SerializedGrabTypes.ContainsKey(group)) throw new Exception($"{type.Name} has the same grab group as {SerializedGrabTypes[group].GetType().Name}, we can't replace grab types!");

                FusionLogger.Log($"Registered {type.Name}");

                SerializedGrabTypes.Add(group, type);
            }
        }


        public static readonly Dictionary<SyncGroup, Type> SerializedGrabTypes = new Dictionary<SyncGroup, Type>(byte.MaxValue);
    }
}
