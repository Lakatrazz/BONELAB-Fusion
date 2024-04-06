using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.RPC;
using LabFusion.Utilities;
using SLZ.AI;
using SLZ.Zones;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ZoneSpawner))]
    public static class ZoneSpawnerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ZoneSpawner.Spawn))]
        public static bool SpawnPrefix(ZoneSpawner __instance, GameObject playerObject)
        {
            if (!NetworkInfo.HasServer)
            {
                return true;
            }

            if (NetworkInfo.IsServer)
            {
                var transform = __instance.transform;

                bool isSpawningAllowed = __instance.isSpawningAllowed;
                __instance.AllowSpawning(false);

                NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
                {
                    position = transform.position,
                    rotation = transform.rotation,

                    spawnable = __instance.spawnable,

                    spawnCallback = (i) =>
                    {
                        OnSpawnCallback(i, __instance, isSpawningAllowed);
                    },
                });

                return false;
            }
            else
            {
                return false;
            }
        }

        private static void OnSpawnCallback(NetworkAssetSpawner.SpawnCallbackInfo info, ZoneSpawner spawner, bool isSpawningAllowed)
        {
            spawner.InvokeSpawnEvent(info.spawned);

            // Send spawn message
            using var writer = FusionWriter.Create();
            using var data = ZoneSpawnerData.Create(info.syncable.GetId(), spawner);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.ZoneSpawner, writer);
            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);

            spawner.AllowSpawning(isSpawningAllowed);
        }
    }
}
