using HarmonyLib;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Audio;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow;
using LabFusion.RPC;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Patching;

// WeaponSpawners are the UI spawners used in Arena and other modes
[HarmonyPatch(typeof(WeaponSpawner))]
public static class WeaponSpawnerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(WeaponSpawner.SpawnDefaultAmmo))]
    public static bool SpawnDefaultAmmoPrefix(WeaponSpawner __instance)
    {
        if (CrossSceneManager.InUnsyncedScene())
        {
            return true;
        }

        // Only the scene host should control the WeaponSpawner
        if (!CrossSceneManager.IsSceneHost())
        {
            return false;
        }

        __instance.DespawnAllAmmo();

        var staticProfiles = StaticProfiles.instance;

        if (!__instance.lightAmmoPoolee)
        {
            var spawnTarg = __instance.lightAmmoSpawnTarg.transform;

            NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
            {
                spawnable = staticProfiles.lightAmmoSpawnable,
                spawnEffect = false,
                position = spawnTarg.position,
                rotation = spawnTarg.rotation,
                spawnCallback = (info) =>
                {
                    var poolee = Poolee.Cache.Get(info.spawned);

                    __instance.lightAmmoPoolee = poolee;
                }
            });
        }

        if (!__instance.medAmmoPoolee)
        {
            var spawnTarg = __instance.medAmmoSpawnTarg.transform;

            NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
            {
                spawnable = staticProfiles.medAmmoSpawnable,
                spawnEffect = false,
                position = spawnTarg.position,
                rotation = spawnTarg.rotation,
                spawnCallback = (info) =>
                {
                    var poolee = Poolee.Cache.Get(info.spawned);

                    __instance.medAmmoPoolee = poolee;
                }
            });
        }

        if (!__instance.heavyAmmoPoolee)
        {
            var spawnTarg = __instance.heavyAmmoSpawnTarg.transform;

            NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
            {
                spawnable = staticProfiles.heavyAmmoSpawnable,
                spawnEffect = false,
                position = spawnTarg.position,
                rotation = spawnTarg.rotation,
                spawnCallback = (info) =>
                {
                    var poolee = Poolee.Cache.Get(info.spawned);

                    __instance.heavyAmmoPoolee = poolee;
                }
            });
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WeaponSpawner.OnWeaponSelected))]
    public static bool OnWeaponSelectedPrefix(WeaponSpawner __instance, SpawnableCrateReference scr)
    {
        if (CrossSceneManager.InUnsyncedScene())
        {
            return true;
        }

        // Only the scene host should control the WeaponSpawner
        if (!CrossSceneManager.IsSceneHost())
        {
            return false;
        }

        if (__instance.pooleeDict.Count >= __instance.weaponLimit)
        {
            SafeAudio3dPlayer.PlayAtPoint(__instance.overLimitClip, __instance.defSpawn.position, Audio3dManager.hardInteraction);
            return false;
        }

        if (!__instance.isSpawningAllowed)
        {
            return false;
        }

        if (__instance.weaponPackMap == null)
        {
            __instance.CreateSpawnDictionary();
        }

        var spawnable = new Spawnable()
        {
            crateRef = scr,
            policyData = null,
        };

        if (__instance.weaponPackMap.TryGetValue(scr.Barcode, out var weaponPack))
        {
            // Don't spawn if theres an existing weapon
            if (weaponPack.isSpawned)
            {
                return false;
            }

            SpawnWeaponPack(__instance, weaponPack);
        }
        else
        {
            NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
            {
                spawnable = spawnable,
                spawnEffect = false,
                position = __instance.defSpawn.position,
                rotation = __instance.defSpawn.rotation,
                spawnCallback = (info) =>
                {
                    OnWeaponSpawned(__instance, info.spawned);
                }
            });
        }

        return false;
    }

    private static void SpawnWeaponPack(WeaponSpawner spawner, WeaponPack pack)
    {
        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
        {
            spawnable = pack.weaponSpawnable,
            spawnEffect = false,
            position = pack.weaponSpawn.position,
            rotation = pack.weaponSpawn.rotation,
            spawnCallback = (info) =>
            {
                var spawned = info.spawned;

                var poolee = Poolee.Cache.Get(spawned);
                var host = InteractableHost.Cache.Get(spawned);

                pack.spawnedWeaponObj = spawned;
                pack.weaponHost = host;
                pack.weaponGrip = host.GetGrip();
                pack.weaponPoolee = poolee;
                pack.OnSpawnWeapon();

                OnWeaponSpawned(spawner, spawned);

                // Spawn the secondary pack if it's not past the weapon limit
                var secondaryPack = pack.secondaryPack;

                if (secondaryPack != null && !secondaryPack.isSpawned && spawner.pooleeDict.Count < spawner.weaponLimit)
                {
                    SpawnWeaponPack(spawner, secondaryPack);
                }
            },
        });
    }

    private static void OnWeaponSpawned(WeaponSpawner spawner, GameObject spawned)
    {
        var poolee = Poolee.Cache.Get(spawned);

        spawner.pooleeDict[spawned] = poolee;

        WeaponSpawner.OnCountUpdated?.Invoke(new Vector2(spawner.pooleeDict.Count, spawner.weaponLimit));

        var position = spawned.transform.position;

        // SFX and VFX
        SafeAudio3dPlayer.PlayAtPoint(spawner.spawnClip, position, Audio3dManager.hardInteraction);

        SafeAssetSpawner.Spawn(spawner.spawnFXSpawnable, position, Quaternion.identity, null);
    }
}
