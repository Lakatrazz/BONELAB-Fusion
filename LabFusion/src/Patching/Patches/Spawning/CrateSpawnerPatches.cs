using HarmonyLib;

using LabFusion.Network;
using LabFusion.RPC;
using LabFusion.Senders;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Pool;

using UnityEngine;

namespace LabFusion.Patching
{
    // SpawnSpawnableAsync is used by the regular SpawnSpawnable as well, so we don't need to patch that
    [HarmonyPatch(typeof(CrateSpawner._SpawnSpawnableAsync_d__23))]
    public static class CrateSpawnerAsyncPatches
    {
        private static void NetworkedSpawnSpawnable(CrateSpawner spawner)
        {
            var spawnable = spawner._spawnable;
            var transform = spawner.transform;

            NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
            {
                spawnable = spawnable,
                position = transform.position,
                rotation = transform.rotation,
                spawnCallback = (info) =>
                {
                    OnNetworkSpawn(spawner, info);
                },
            });
        }

        private static void OnNetworkSpawn(CrateSpawner spawner, NetworkAssetSpawner.SpawnCallbackInfo info)
        {
            var spawned = info.spawned;
            spawner.OnFinishNetworkSpawn(spawned);

            // Send spawn message
            var spawnedId = info.entity.Id;

            SpawnSender.SendCratePlacerEvent(spawner, spawnedId);
        }

        public static void OnFinishNetworkSpawn(this CrateSpawner spawner, GameObject go)
        {
            var poolee = Poolee.Cache.Get(go);

            spawner.OnPooleeSpawn(go);

            poolee.OnDespawnDelegate += (Action<GameObject>)spawner.OnPooleeDespawn;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CrateSpawner._SpawnSpawnableAsync_d__23.MoveNext))]
        public static bool MoveNext(CrateSpawner._SpawnSpawnableAsync_d__23 __instance)
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
            var spawner = __instance.__4__this;
            NetworkedSpawnSpawnable(spawner);

            return false;
        }
    }
}
