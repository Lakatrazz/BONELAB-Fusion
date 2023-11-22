using System;
using System.Collections;
using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Syncables;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Marrow.Warehouse;
using UnityEngine;

namespace LabFusion.Network
{
    public class SpawnableCratePlacerData : IFusionSerializable, IDisposable
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static SpawnableCratePlacerData Create(ushort spawnId, GameObject placer)
        {
            return new SpawnableCratePlacerData
            {
                spawnId = spawnId,
                placer = placer,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class SpawnableCratePlacerMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SpawnableCratePlacer;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            using var data = reader.ReadFusionSerializable<SpawnableCratePlacerData>();
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
            ISyncable syncable = null;
            while (syncable == null && TimeUtilities.TimeSinceStartup - startTime <= 1f)
            {
                yield return null;

                SyncManager.TryGetSyncable(spawnId, out syncable);
            }

            if (syncable == null)
                yield break;

            var cratePlacer = placer.GetComponentInChildren<SpawnableCratePlacer>(true);

            if (cratePlacer)
                cratePlacer.OnPlaceEvent?.Invoke(cratePlacer, ((PropSyncable)syncable).GameObject);
        }
    }
}
