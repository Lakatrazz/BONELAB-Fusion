using HarmonyLib;

using LabFusion.Network;
using LabFusion.RPC;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Warehouse;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(CrateSpawner))]
    public static class CrateSpawnerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CrateSpawner.SpawnSpawnable))]
        public static bool SpawnSpawnable(CrateSpawner __instance)
        {
            // If there is NO server, the spawner can function as normal.
            if (!NetworkInfo.HasServer)
            {
                return true;
            }

            // If we aren't the server, don't allow a crate spawn
            if (!NetworkInfo.IsServer)
            {
                return false;
            }

            // Otherwise, manually sync this spawn over the network
            var spawnable = __instance._spawnable;
            var transform = __instance.transform;

            NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
            {
                spawnable = spawnable,
                position = transform.position,
                rotation = transform.rotation,
                spawnCallback = (info) =>
                {
                    OnNetworkSpawn(__instance, info);
                },
            });

            return false;
        }

        private static void OnNetworkSpawn(CrateSpawner spawner, NetworkAssetSpawner.SpawnCallbackInfo info)
        {
            var spawned = info.spawned;

            spawner.OnPooleeSpawn(spawned);
        }
    }
}
