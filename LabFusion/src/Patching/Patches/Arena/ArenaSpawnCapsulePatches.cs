using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Network;
using LabFusion.RPC;

using UnityEngine;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(Arena_SpawnCapsule._CoLaunchSequenceArena_d__9))]
public static class SpawnCapsuleLaunchSequencePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_SpawnCapsule._CoLaunchSequenceArena_d__9.MoveNext))]
    public static bool MoveNext(Arena_SpawnCapsule._CoLaunchSequenceArena_d__9 __instance)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        // Manually sync spawn
        if (NetworkInfo.IsServer)
        {
            var arenaGameController = __instance.arenaGameController;
            var spawnCapsule = __instance.__4__this;
            var transform = spawnCapsule.transform;

            var enemyProfile = __instance.enemyProfile;

            // In the future, add this to the game controller
            NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
            {
                spawnable = enemyProfile.spawnable,
                position = spawnCapsule.transform.position,
                rotation = Quaternion.identity,
                spawnCallback = (info) =>
                {
                    spawnCapsule.OnSpawn?.Invoke();
                }
            });
        }

        return false;
    }
}