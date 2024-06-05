using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;

using SLZ.Marrow.Pool;

using UnityEngine;
using SLZ.Props.Weapons;
using SLZ.Rig;

namespace LabFusion.Utilities
{
    public static class PooleeUtilities
    {
        internal static PooleePusher ForceEnabled = new();

        internal static PooleePusher CheckingForSpawn = new();

        internal static PooleePusher CanSpawnList = new();

        internal static bool CanDespawn = false;

        internal static PooleePusher ServerSpawnedList = new();

        public static void DespawnAll()
        {
            if (NetworkInfo.IsServer)
            {
                var pools = AssetSpawner._instance._poolList;

                // Loop through all pools and get their spawned objects so we can despawn them
                foreach (var pool in pools)
                {
                    var spawnedObjects = pool.spawned.ToArray();

                    foreach (var spawned in spawnedObjects)
                    {
                        // Don't despawn the player!
                        if (spawned.GetComponentInChildren<RigManager>(true) != null)
                            continue;

                        // Also don't despawn magazines (or guns with magazines)
                        if (spawned.GetComponentInChildren<Magazine>(true) != null)
                            continue;

                        spawned.Despawn();
                    }
                }
            }
        }

        public static void OnServerLocalSpawn(ushort syncId, GameObject go, out PropSyncable newSyncable)
        {
            newSyncable = null;

            if (!NetworkInfo.IsServer)
                return;

            if (PropSyncable.Cache.TryGet(go, out var syncable))
                SyncManager.RemoveSyncable(syncable);

            if (!AssetPoolee.Cache.Get(go))
                go.AddComponent<AssetPoolee>();

            newSyncable = new PropSyncable(null, go.gameObject);
            newSyncable.SetOwner(0);

            SyncManager.RegisterSyncable(newSyncable, syncId);
        }

        public static bool IsPlayer(AssetPoolee poolee)
        {
            if (poolee.IsNOC())
                return false;

            return poolee.GetComponentInChildren<RigManager>(true);
        }

        public static void SendDespawn(ushort syncId)
        {
            // Send response
            if (NetworkInfo.IsServer)
            {
                using var writer = FusionWriter.Create(DespawnResponseData.Size);
                var data = DespawnResponseData.Create(syncId, PlayerIdManager.LocalSmallId);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.DespawnResponse, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
            // Send request
            else
            {
                using var writer = FusionWriter.Create(DespawnRequestData.Size);
                var data = DespawnRequestData.Create(syncId, PlayerIdManager.LocalSmallId);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.DespawnRequest, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
        }

        public static void RequestDespawn(ushort syncId, bool isMag = false)
        {
            using var writer = FusionWriter.Create(DespawnRequestData.Size);
            var data = DespawnRequestData.Create(syncId, PlayerIdManager.LocalSmallId, isMag);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.DespawnRequest, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        public static void RequestSpawn(string barcode, SerializedTransform serializedTransform, uint trackerId)
        {
            using var writer = FusionWriter.Create(SpawnRequestData.Size);
            var data = SpawnRequestData.Create(PlayerIdManager.LocalSmallId, barcode, serializedTransform, trackerId);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.SpawnRequest, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        public static void SendSpawn(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, bool ignoreSelf = false, uint trackerId = 0)
        {
            using var writer = FusionWriter.Create(SpawnResponseData.GetSize(barcode));
            var data = SpawnResponseData.Create(owner, barcode, syncId, serializedTransform, trackerId);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.SpawnResponse, writer);
            if (!ignoreSelf)
                MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
            else
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
        }

        public static bool CanForceDespawn(AssetPoolee instance)
        {
            return !CanSpawnList.Pull(instance) && instance.gameObject.IsSyncWhitelisted();
        }

        public static bool CanSendSpawn(AssetPoolee instance)
        {
            return instance.gameObject.IsSyncWhitelisted();
        }
    }
}
