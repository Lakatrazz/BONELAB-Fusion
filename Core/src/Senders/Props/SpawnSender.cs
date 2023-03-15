using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using MelonLoader;

using LabFusion.Network;
using LabFusion.Data;

using SLZ.Zones;
using SLZ;
using LabFusion.Syncables;
using LabFusion.Utilities;
using SLZ.Marrow.Warehouse;

namespace LabFusion.Senders
{
    public static class SpawnSender
    {
        /// <summary>
        /// Sends a catchup for the OnPlaceEvent for a SpawnableCratePlacer.
        /// </summary>
        /// <param name="placer"></param>
        /// <param name="syncable"></param>
        /// <param name="userId"></param>
        public static void SendCratePlacerCatchup(SpawnableCratePlacer placer, PropSyncable syncable, ulong userId) {
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create(SpawnableCratePlacerData.Size))
                {
                    using (var data = SpawnableCratePlacerData.Create(syncable.GetId(), placer.gameObject))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.SpawnableCratePlacer, writer))
                        {
                            MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends the OnPlaceEvent for a SpawnableCratePlacer.
        /// </summary>
        /// <param name="placer"></param>
        /// <param name="go"></param>
        public static void SendCratePlacerEvent(SpawnableCratePlacer placer, GameObject go)
        {
            if (NetworkInfo.IsServer)
            {
                MelonCoroutines.Start(Internal_WaitForCratePlacer(placer, go));
            }
        }

        private static IEnumerator Internal_WaitForCratePlacer(SpawnableCratePlacer placer, GameObject go)
        {
            while (FusionSceneManager.IsLoading())
                yield return null;

            for (var i = 0; i < 5; i++)
                yield return null;

            if (PropSyncable.Cache.TryGet(go, out var syncable))
            {
                using (var writer = FusionWriter.Create(SpawnableCratePlacerData.Size))
                {
                    using (var data = SpawnableCratePlacerData.Create(syncable.GetId(), placer.gameObject))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.SpawnableCratePlacer, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }

                // Insert the catchup hook for future users
                syncable.InsertCatchupDelegate((id) => {
                    SendCratePlacerCatchup(placer, syncable, id);
                });
            }
        }


        /// <summary>
        /// Sends a catchup sync message for a pool spawned object.
        /// </summary>
        /// <param name="syncable"></param>
        public static void SendCatchupSpawn(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, ZoneSpawner spawner, Handedness hand, ulong userId)
        {
            if (NetworkInfo.IsServer)
            {
                using (var writer = FusionWriter.Create(SpawnResponseData.Size))
                {
                    using (var data = SpawnResponseData.Create(owner, barcode, syncId, serializedTransform, spawner, hand))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.SpawnResponse, writer))
                        {
                            MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
