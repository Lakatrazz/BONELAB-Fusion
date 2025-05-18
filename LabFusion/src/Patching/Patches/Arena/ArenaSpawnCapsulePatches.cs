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
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        // If we aren't the host, don't allow a spawn
        if (!NetworkInfo.IsServer)
        {
            return false;
        }

        // Manually sync spawn
        var arenaGameController = __instance.arenaGameController;
        var spawnCapsule = __instance.__4__this;
        var transform = spawnCapsule.transform;

        var enemyProfile = __instance.enemyProfile;

        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
        {
            Spawnable = enemyProfile.spawnable,
            Position = spawnCapsule.transform.position,
            Rotation = Quaternion.identity,
            SpawnCallback = (info) =>
            {
                spawnCapsule.OnSpawn?.Invoke();

                // Call spawn event for the spawned NPC
                var spawnEvent = new Arena_SpawnCapsule.__c__DisplayClass9_0()
                {
                    arenaGameController = arenaGameController,
                    enemyProfile = __instance.enemyProfile,
                };

                spawnEvent._CoLaunchSequenceArena_b__0(info.Spawned);

                // Disable the hoi-poi
                spawnCapsule.gameObject.SetActive(false);
            }
        });

        return false;
    }
}