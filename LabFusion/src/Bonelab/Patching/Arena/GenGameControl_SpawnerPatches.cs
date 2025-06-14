using HarmonyLib;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Data;

using LabFusion.RPC;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GenGameControl_Spawner))]
public static class GenGameControl_SpawnerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GenGameControl_Spawner.SpawnWaveLoot))]
    public static bool SpawnWaveLootPrefix(GenGameControl_Spawner __instance, WaveProfile wave)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelHost)
        {
            return false;
        }

        if (!__instance.arenaGameController)
        {
            return false;
        }

        OnSpawnWaveLoot(__instance, wave);

        return false;
    }

    private static void OnSpawnWaveLoot(GenGameControl_Spawner spawner, WaveProfile wave)
    {
        var arenaGameController = spawner.arenaGameController;

        var assignedRoundProfile = arenaGameController.assignedRoundProfile;

        var staticProfiles = StaticProfiles.instance;

        if (assignedRoundProfile.arenaGameMode != RoundProfileGroup.ArenaGameMode.WEAPON)
        {
            foreach (var lootItem in wave.lootItems)
            {
                SpawnLootItem(spawner, lootItem);
            }

            SpawnAmmoBoxes(spawner, wave, staticProfiles);
        }
        else
        {
            // TODO: implement weapon mode
        }
    }

    private static void SpawnLootItem(GenGameControl_Spawner spawner, ArenaLootItem lootItem)
    {
        float random = UnityEngine.Random.Range(0f, 100f);

        if (random > lootItem.percentage)
        {
            return;
        }

        var spawnable = lootItem.spawnable;

        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
        {
            Spawnable = spawnable,
            Position = spawner.transform.position,
            Rotation = Quaternion.identity,
            SpawnEffect = true,
            SpawnCallback = (info) =>
            {
                spawner.spawnedWeaponObjList.Add(info.Spawned);
            },
        });
    }

    private static void SpawnAmmoBoxes(GenGameControl_Spawner spawner, WaveProfile wave, StaticProfiles staticProfiles)
    {
        bool alreadySpawnedLight = spawner.spawnedLightPickup != null && spawner.spawnedLightPickup.isActiveAndEnabled;
        bool alreadySpawnedMed = spawner.spawnedLightPickup != null && spawner.spawnedLightPickup.isActiveAndEnabled;
        bool alreadySpawnedHeavy = spawner.spawnedLightPickup != null && spawner.spawnedLightPickup.isActiveAndEnabled;

        if (!alreadySpawnedLight)
        {
            SpawnAmmoBox(spawner, staticProfiles.lightAmmoSpawnable, (pickup) =>
            {
                pickup.ammoCount = wave.lightBulletAmount;

                spawner.lightAmmoSpawned += wave.lightBulletAmount;
                spawner.spawnedLightPickup = pickup;
                spawner.ammoPickupList.Add(pickup);
            });
        }

        if (!alreadySpawnedMed)
        {
            SpawnAmmoBox(spawner, staticProfiles.medAmmoSpawnable, (pickup) =>
            {
                pickup.ammoCount = wave.medBulletAmount;

                spawner.medAmmoSpawned += wave.medBulletAmount;
                spawner.spawnedMedPickup = pickup;
                spawner.ammoPickupList.Add(pickup);
            });
        }

        if (!alreadySpawnedHeavy)
        {
            SpawnAmmoBox(spawner, staticProfiles.heavyAmmoSpawnable, (pickup) =>
            {
                pickup.ammoCount = wave.heavyBulletAmount;

                spawner.heavyAmmoSpawned += wave.heavyBulletAmount;
                spawner.spawnedHeavyPickup = pickup;
                spawner.ammoPickupList.Add(pickup);
            });
        }
    }

    private static void SpawnAmmoBox(GenGameControl_Spawner spawner, Spawnable spawnable, Action<AmmoPickup> callback)
    {
        var spawnTransform = spawner.ammoSpawnTrans;

        if (spawnTransform == null)
        {
            spawnTransform = spawner.transform;
        }

        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
        {
            Spawnable = spawnable,
            Position = spawnTransform.position,
            Rotation = spawnTransform.rotation,
            SpawnEffect = true,
            SpawnCallback = (info) =>
            {
                var pickup = info.Spawned.GetComponentInChildren<AmmoPickup>();

                if (pickup == null)
                {
                    return;
                }

                callback?.Invoke(pickup);
            }
        });
    }
}
