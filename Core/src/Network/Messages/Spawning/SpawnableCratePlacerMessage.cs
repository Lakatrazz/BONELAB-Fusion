using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Syncables;
using LabFusion.Patching;

using SLZ;
using SLZ.Interaction;

using LabFusion.Extensions;

using UnityEngine;

using LabFusion.Exceptions;
using SLZ.Marrow.Warehouse;

using System.Collections;

using MelonLoader;

using LabFusion.Senders;

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
        public override byte? Tag => NativeMessageTag.SpawnableCratePlacer;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<SpawnableCratePlacerData>())
                {
                    // This should only be handled by clients
                    if (!NetworkInfo.IsServer && !isServerHandled) {
                        if (data.placer != null) {
                            MelonCoroutines.Start(Internal_WaitForSyncable(data.placer, data.spawnId));
                        }
                    }
                    else
                        throw new ExpectedClientException();
                }
            }
        }

        private static IEnumerator Internal_WaitForSyncable(GameObject placer, ushort spawnId) {
            float startTime = Time.realtimeSinceStartup;
            ISyncable syncable = null;
            while (syncable == null && Time.realtimeSinceStartup - startTime <= 1f) {
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
