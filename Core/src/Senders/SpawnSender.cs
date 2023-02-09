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

namespace LabFusion.Senders
{
    public static class SpawnSender
    {
        /// <summary>
        /// Sends a catchup sync message for a pool spawned object.
        /// </summary>
        /// <param name="syncable"></param>
        public static void SendCatchupSpawn(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, ZoneSpawner spawner, Handedness hand, ulong userId)
        {
            if (NetworkInfo.IsServer)
            {
                using (var writer = FusionWriter.Create())
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
