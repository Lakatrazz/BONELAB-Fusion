using System;

using LabFusion.Data;
using LabFusion.Syncables;
using LabFusion.Extensions;

using UnityEngine;

using SLZ.Zones;

namespace LabFusion.Network
{
    public class ZoneSpawnerData : IFusionSerializable
    {
        public ushort syncId;

        public GameObject zoneSpawner;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(syncId);

            writer.Write(zoneSpawner);
        }

        public void Deserialize(FusionReader reader)
        {
            syncId = reader.ReadUInt16();

            zoneSpawner = reader.ReadGameObject();
        }

        public static ZoneSpawnerData Create(ushort syncId, ZoneSpawner zoneSpawner)
        {
            return new ZoneSpawnerData()
            {
                syncId = syncId,
                zoneSpawner = zoneSpawner.gameObject,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class ZoneSpawnerMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ZoneSpawner;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<ZoneSpawnerData>();

            // We ONLY handle this if we are a client!
            if (!NetworkInfo.IsServer && SyncManager.TryGetSyncable<PropSyncable>(data.syncId, out var syncable) && data.zoneSpawner != null)
            {
                var spawner = data.zoneSpawner.GetComponent<ZoneSpawner>();
                var spawned = syncable.GameObject;

                spawner.InvokeSpawnEvent(spawned);
            }
        }
    }
}
