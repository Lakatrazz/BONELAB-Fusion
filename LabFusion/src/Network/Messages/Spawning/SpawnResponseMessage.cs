using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Pool;

using UnityEngine;

using LabFusion.Syncables;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Data;

using LabFusion.Extensions;

using LabFusion.Exceptions;
using LabFusion.Senders;
using LabFusion.RPC;
using LabFusion.Marrow;

namespace LabFusion.Network
{
    public class SpawnResponseData : IFusionSerializable
    {
        public const int DefaultSize = sizeof(byte) * 2 + sizeof(ushort) + SerializedTransform.Size;

        public byte owner;
        public string barcode;
        public ushort syncId;

        public SerializedTransform serializedTransform;

        public uint trackerId;

        public static int GetSize(string barcode)
        {
            return DefaultSize + barcode.GetSize();
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(owner);
            writer.Write(barcode);
            writer.Write(syncId);
            writer.Write(serializedTransform);

            writer.Write(trackerId);
        }

        public void Deserialize(FusionReader reader)
        {
            owner = reader.ReadByte();
            barcode = reader.ReadString();
            syncId = reader.ReadUInt16();
            serializedTransform = reader.ReadFusionSerializable<SerializedTransform>();

            trackerId = reader.ReadUInt32();
        }

        public static SpawnResponseData Create(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, uint trackerId = 0)
        {
            return new SpawnResponseData()
            {
                owner = owner,
                barcode = barcode,
                syncId = syncId,
                serializedTransform = serializedTransform,
                trackerId = trackerId,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class SpawnResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SpawnResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (isServerHandled)
            {
                throw new ExpectedClientException();
            }

            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<SpawnResponseData>();
            var crateRef = new SpawnableCrateReference(data.barcode);

            var spawnable = new Spawnable()
            {
                crateRef = crateRef,
                policyData = null
            };

            AssetSpawner.Register(spawnable);

            byte owner = data.owner;
            string barcode = data.barcode;
            ushort syncId = data.syncId;
            var trackerId = data.trackerId;

            Action<Poolee> onSpawnFinished = (go) =>
            {
                OnSpawnFinished(owner, barcode, syncId, go, trackerId);
            };

            SafeAssetSpawner.Spawn(spawnable, data.serializedTransform.position, data.serializedTransform.rotation, onSpawnFinished);
        }

        public static void OnSpawnFinished(byte owner, string barcode, ushort syncId, Poolee poolee, uint trackerId = 0)
        {
            var go = poolee.gameObject;

            if (PropSyncable.Cache.TryGet(go, out var syncable))
                SyncManager.RemoveSyncable(syncable);

            PooleeUtilities.CheckingForSpawn.Push(poolee);

            PropSyncable newSyncable = new(null, go.gameObject);
            newSyncable.SetOwner(owner);

            SyncManager.RegisterSyncable(newSyncable, syncId);

            // If we are the server, insert the catchup hook for future users
            if (NetworkInfo.IsServer)
                newSyncable.InsertCatchupDelegate((id) =>
                {
                    SpawnSender.SendCatchupSpawn(owner, barcode, syncId, new SerializedTransform(go.transform), id);
                });

            // Invoke spawn callback
            if (owner == PlayerIdManager.LocalSmallId)
            {
                NetworkAssetSpawner.OnSpawnComplete(trackerId, new NetworkAssetSpawner.SpawnCallbackInfo()
                {
                    spawned = go,
                    syncable = newSyncable,
                });
            }

            DelayUtilities.Delay(() => { Internal_PostSpawn(poolee); }, 3);
        }

        private static void Internal_PostSpawn(Poolee __instance)
        {
            PooleeUtilities.CheckingForSpawn.Pull(__instance);
        }
    }
}
