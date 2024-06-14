using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Syncables;
using LabFusion.Exceptions;

using UnityEngine;

using Il2CppSLZ.Marrow.Warehouse;

using System.Collections;

using MelonLoader;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public class CrateSpawnerData : IFusionSerializable
    {
        public const int Size = sizeof(ushort);

        public ushort spawnedId;
        public GameObject placer;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(spawnedId);
            writer.Write(placer);
        }

        public void Deserialize(FusionReader reader)
        {
            spawnedId = reader.ReadUInt16();
            placer = reader.ReadGameObject();
        }

        public static CrateSpawnerData Create(ushort spawnedId, GameObject placer)
        {
            return new CrateSpawnerData()
            {
                spawnedId = spawnedId,
                placer = placer,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class CrateSpawnerMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.CrateSpawner;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<CrateSpawnerData>();

            // This should only be handled by clients
            if (isServerHandled)
            {
                throw new ExpectedClientException();
            }

            if (data.placer != null)
            {
                MelonCoroutines.Start(Internal_WaitForSyncable(data.placer, data.spawnedId));
            }
        }

        private static IEnumerator Internal_WaitForSyncable(GameObject placer, ushort spawnId)
        {
            float startTime = TimeUtilities.TimeSinceStartup;
            PropSyncable syncable = null;
            while (syncable == null && TimeUtilities.TimeSinceStartup - startTime <= 1f)
            {
                yield return null;

                SyncManager.TryGetSyncable(spawnId, out syncable);
            }

            if (syncable == null)
                yield break;

            var crateSpawner = placer.GetComponentInChildren<CrateSpawner>(true);

            if (crateSpawner)
            {
                crateSpawner.OnFinishNetworkSpawn(syncable.Poolee.gameObject);
            }
        }
    }
}
