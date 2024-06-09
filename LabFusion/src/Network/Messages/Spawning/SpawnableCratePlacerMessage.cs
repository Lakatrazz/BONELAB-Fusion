using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Syncables;

using UnityEngine;

using LabFusion.Exceptions;
using Il2CppSLZ.Marrow.Warehouse;

using System.Collections;

using MelonLoader;

namespace LabFusion.Network
{
    public class SpawnableCratePlacerData : IFusionSerializable
    {
        public const int Size = sizeof(ushort);

        public ushort spawnId;
        public GameObject placer;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(spawnId);
            writer.Write(placer);
        }

        public void Deserialize(FusionReader reader)
        {
            spawnId = reader.ReadUInt16();
            placer = reader.ReadGameObject();
        }

        public static SpawnableCratePlacerData Create(ushort spawnId, GameObject placer)
        {
            return new SpawnableCratePlacerData()
            {
                spawnId = spawnId,
                placer = placer,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class SpawnableCratePlacerMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.CrateSpawner;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<SpawnableCratePlacerData>();
            // This should only be handled by clients
            if (!NetworkInfo.IsServer && !isServerHandled)
            {
                if (data.placer != null)
                {
                    MelonCoroutines.Start(Internal_WaitForSyncable(data.placer, data.spawnId));
                }
            }
            else
                throw new ExpectedClientException();
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

            var cratePlacer = placer.GetComponentInChildren<CrateSpawner>(true);

            if (cratePlacer)
                cratePlacer.onSpawnEvent?.Invoke(cratePlacer, syncable.GameObject);
        }
    }
}
