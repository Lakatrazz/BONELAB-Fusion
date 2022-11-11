using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Marrow.Pool;
using SLZ.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    public static class PooleeUtilities {
        internal static List<AssetPoolee> CanSpawnList = new List<AssetPoolee>();

        internal static bool CanDespawn = false;

        internal static List<AssetPoolee> ServerSpawnedList = new List<AssetPoolee>();

        public static void AddToServer(AssetPoolee poolee) {
            if (!ServerSpawnedList.Has(poolee))
                ServerSpawnedList.Add(poolee);
        }

        public static bool DequeueServerSpawned(AssetPoolee poolee)
        {
            for (var i = 0; i < ServerSpawnedList.Count; i++) {
                var found = ServerSpawnedList[i];

                if (poolee == found) {
                    ServerSpawnedList.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public static void PermitSpawning(AssetPoolee poolee) {
            if (!CanSpawnList.Contains(poolee))
                CanSpawnList.Add(poolee);
        }

        public static bool CanSpawn(AssetPoolee poolee) {
            return CanSpawnList.Contains(poolee);
        }

        public static bool DequeueSpawning(AssetPoolee poolee) {
            return CanSpawnList.Remove(poolee);
        }

        public static void SendDespawn(ushort syncId) {
            // Send response
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create()) {
                    using (var data = DespawnResponseData.Create(syncId)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.DespawnResponse, writer)) {
                            MessageSender.BroadcastMessageExcept(0, NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
            // Send request
            else {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = DespawnRequestData.Create(syncId))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.DespawnRequest, writer))
                        {
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static void RequestSpawn(string barcode, SerializedTransform serializedTransform, byte? owner = null) {
            if (NetworkInfo.IsServer)
                return;

            using (var writer = FusionWriter.Create())
            {
                using (var data = SpawnRequestData.Create(owner.HasValue ? owner.Value : PlayerIdManager.LocalSmallId, barcode, serializedTransform))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.SpawnRequest, writer)) {
                        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendSpawn(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, bool ignoreSelf = false, ZoneSpawner spawner = null) {
            if (!NetworkInfo.IsServer)
                return;

            using (var writer = FusionWriter.Create()) {
                using (var data = SpawnResponseData.Create(owner, barcode, syncId, serializedTransform, spawner)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.SpawnResponse, writer)) {
                        if (!ignoreSelf)
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                        else
                            MessageSender.BroadcastMessageExcept(0, NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static bool CanForceDespawn(AssetPoolee instance) {
            return !DequeueSpawning(instance) && instance.GetComponentInChildren<Rigidbody>(true) != null;
        }

        public static bool CanSendSpawn(AssetPoolee instance) {
            return instance.GetComponentInChildren<Rigidbody>(true) != null;
        }
    }
}
