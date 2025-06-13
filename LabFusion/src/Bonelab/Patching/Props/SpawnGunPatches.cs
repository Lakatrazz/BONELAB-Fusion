using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.SDK.Achievements;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Bonelab.Messages;
using LabFusion.Bonelab.Extenders;

using HarmonyLib;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(SpawnGun))]
public static class SpawnGunPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpawnGun.OnTriggerGripAttached))]
    public static void OnTriggerGripAttachedPrefix(Hand hand)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (!hand.manager.IsLocalPlayer())
        {
            PopUpMenuViewPatches.DisableMethods = true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SpawnGun.OnTriggerGripAttached))]
    public static void OnTriggerGripAttachedPostfix()
    {
        PopUpMenuViewPatches.DisableMethods = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpawnGun.OnTriggerGripDetached))]
    public static void OnTriggerGripDetachedPrefix(Hand hand)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (!hand.manager.IsLocalPlayer())
        {
            PopUpMenuViewPatches.DisableMethods = true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SpawnGun.OnTriggerGripDetached))]
    public static void OnTriggerGripDetachedPostfix()
    {
        PopUpMenuViewPatches.DisableMethods = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SpawnGun.OnSpawnableSelected))]
    public static void OnSpawnableSelected(SpawnGun __instance, SpawnableCrate crate)
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var entity = SpawnGunExtender.Cache.Get(__instance);

        if (entity == null || !entity.IsOwner)
        {
            return;
        }

        string barcode = Barcode.EMPTY;

        if (crate != null)
        {
            barcode = crate.Barcode.ID;
        }

        var data = new SpawnGunSelectData()
        {
            SpawnGunID = entity.ID,
            Barcode = barcode,
        };

        MessageRelay.RelayModule<SpawnGunSelectMessage, SpawnGunSelectData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpawnGun.OnFire))]
    public static bool OnFirePrefix(SpawnGun __instance, ref SpawnableCrate __state)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        __state = __instance._selectedCrate;
        __instance._selectedCrate = null;

        if (__instance._selectedMode == UtilityModes.REMOVER)
        {
            OnFireDespawn(__instance);
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SpawnGun.OnFire))]
    public static void OnFirePostfix(SpawnGun __instance, ref SpawnableCrate __state)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        __instance._selectedCrate = __state;

        if (__instance._selectedMode == UtilityModes.SPAWNER)
        {
            OnFireSpawn(__instance);
        }
    }

    private static void OnFireSpawn(SpawnGun spawnGun)
    {
        // Check for prevention
        if (FusionDevTools.PreventSpawnGun(PlayerIDManager.LocalID))
        {
            return;
        }

        var crate = spawnGun._selectedCrate;

        if (crate == null)
        {
            return;
        }

        // Reward achievement
        if (PlayerIDManager.HasOtherPlayers && AchievementManager.TryGetAchievement<LavaGang>(out var achievement))
        {
            achievement.IncrementTask();
        }

        // Send a spawn request
        var spawnable = new Spawnable() { crateRef = new SpawnableCrateReference(crate.Barcode) };
        var transform = spawnGun.placerPreview.transform;

        var info = new NetworkAssetSpawner.SpawnRequestInfo()
        {
            Spawnable = spawnable,
            Position = transform.position,
            Rotation = transform.rotation,
            SpawnEffect = true,
        };

        NetworkAssetSpawner.Spawn(info);
    }

    private static void OnFireDespawn(SpawnGun spawnGun)
    {
        var rigidbody = spawnGun._hitInfo.rigidbody;

        if (rigidbody == null)
        {
            return;
        }

        var marrowBody = MarrowBody.Cache.Get(rigidbody.gameObject);

        if (marrowBody == null)
        {
            return;
        }

        var marrowEntity = marrowBody.Entity;

        // Check if it's a player and prevent despawn
        var tags = marrowEntity.Tags;

        foreach (var tag in tags.Tags)
        {
            var barcode = tag.Barcode;

            if (barcode == spawnGun.playerTag.Barcode)
            {
                return;
            }

            if (barcode == FusionBoneTagReferences.FusionPlayerReference.Barcode)
            {
                return;
            }
        }

        // Reward achievement
        if (PlayerIDManager.HasOtherPlayers && AchievementManager.TryGetAchievement<CleanupCrew>(out var achievement))
        {
            achievement.IncrementTask();
        }

        // Send a despawn request if there's a syncable
        var poolee = Poolee.Cache.Get(marrowEntity.gameObject);

        if (PooleeExtender.Cache.TryGet(poolee, out var entity) && entity.IsRegistered)
        {
            PooleeUtilities.RequestDespawn(entity.ID, true);
        }

        // Flash the spawn gun
        spawnGun.FlashScreen();
        spawnGun.SpawnFlareAsync().Forget();
    }
}