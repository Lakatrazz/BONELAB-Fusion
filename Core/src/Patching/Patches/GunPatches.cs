using System;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.SDK.Achievements;
using LabFusion.Syncables;

using SLZ.Marrow.Pool;
using SLZ.Props;
using SLZ.Props.Weapons;
using SLZ.Interaction;

using MelonLoader;
using LabFusion.RPC;
using SLZ.Marrow.Data;
using SLZ.Marrow.Warehouse;
using BoneLib;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Gun))]
    public static class GunPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPatch(nameof(Gun.Fire))]
        [HarmonyPrefix]
        public static bool Fire(Gun __instance)
        {
            if (IgnorePatches)
            {
                return true;
            }

            if (!NetworkInfo.HasServer)
            {
                return true;
            }

            var grip = __instance.triggerGrip;

            if (grip == null)
            {
                return true;
            }

            var hand = grip.GetHand();

            if (hand == null) 
            {
                return true;
            }

            var manager = hand.manager;

            bool isPlayerRep = PlayerRepManager.HasPlayerId(manager);

            if (isPlayerRep && __instance.cartridgeState == Gun.CartridgeStates.UNSPENT)
            {
                return false;
            }

            var health = manager.health.TryCast<Player_Health>();

            bool isDead = health.deathIsImminent;

            if (isDead)
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(Gun.OnFire))]
        [HarmonyPrefix]
        public static void OnFire(Gun __instance)
        {
            if (IgnorePatches)
                return;

            try
            {
                if (NetworkInfo.HasServer && GunExtender.Cache.TryGet(__instance, out var gunSyncable) && gunSyncable.TryGetExtender<GunExtender>(out var extender))
                {
                    // Make sure this is being grabbed by our main player
                    if (__instance.triggerGrip && __instance.triggerGrip.attachedHands.Find((Il2CppSystem.Predicate<Hand>)((h) => h.manager.IsSelf())))
                    {
                        using var writer = FusionWriter.Create(GunShotData.Size);
                        var ammoCount = __instance._magState != null ? (byte)__instance._magState.AmmoCount : (byte)0;

                        var data = GunShotData.Create(PlayerIdManager.LocalSmallId, ammoCount, gunSyncable.Id, extender.GetIndex(__instance).Value);
                        writer.Write(data);

                        using var message = FusionMessage.Create(NativeMessageTag.GunShot, writer);
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("patching Gun.OnFire", e);
            }
        }
    }

    [HarmonyPatch(typeof(SpawnGun))]
    public static class SpawnGunPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpawnGun.SetPreviewMesh))]
        public static void SetPreviewMesh(SpawnGun __instance)
        {
            if (IgnorePatches)
                return;

            if (__instance._selectedCrate != null && NetworkInfo.HasServer && SpawnGunExtender.Cache.TryGet(__instance, out var syncable))
            {
                string barcode = __instance._selectedCrate.Barcode;

                using var writer = FusionWriter.Create(SpawnGunPreviewMeshData.GetSize(barcode));
                var data = SpawnGunPreviewMeshData.Create(PlayerIdManager.LocalSmallId, syncable.GetId(), barcode);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.SpawnGunPreviewMesh, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpawnGun.OnFire))]
        public static void OnFirePrefix(SpawnGun __instance, ref SpawnableCrate __state)
        {
            if (!NetworkInfo.HasServer)
            {
                return;
            }

            __state = __instance._selectedCrate;
            __instance._selectedCrate = null;
        }

        private static void OnFireSpawn(SpawnGun spawnGun)
        {
            // Check for prevention
            if (FusionDevTools.PreventSpawnGun(PlayerIdManager.LocalId))
            {
                return;
            }

            var crate = spawnGun._selectedCrate;

            if (crate == null)
            {
                return;
            }

            // Reward achievement
            if (PlayerIdManager.HasOtherPlayers && AchievementManager.TryGetAchievement<LavaGang>(out var achievement))
                achievement.IncrementTask();

            // Send a spawn request
            var spawnable = new Spawnable() { crateRef = new SpawnableCrateReference(crate.Barcode) };
            var transform = spawnGun.placerPreview.transform;
            
            var info = new NetworkAssetSpawner.SpawnRequestInfo()
            {
                spawnable = spawnable,
                position = transform.position,
                rotation = transform.rotation,
            };
            
            NetworkAssetSpawner.Spawn(info);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpawnGun.OnFire))]
        public static void OnFirePostfix(SpawnGun __instance, ref SpawnableCrate __state)
        {
            if (!NetworkInfo.HasServer)
            {
                return;
            }

            __instance._selectedCrate = __state;

            if (__instance._selectedMode == UtilityModes.SPAWNER)
            {
                OnFireSpawn(__instance);
            }
            else if (__instance._selectedMode == UtilityModes.REMOVER && __instance._hitInfo.rigidbody != null)
            {
                var hitBody = __instance._hitInfo.rigidbody;
                AssetPoolee poolee = hitBody.GetComponentInParent<AssetPoolee>();

                if (poolee != null)
                {
                    // Reward achievement
                    if (PlayerIdManager.HasOtherPlayers && AchievementManager.TryGetAchievement<CleanupCrew>(out var achievement))
                        achievement.IncrementTask();

                    // No need to send a despawn request if we are the server
                    if (!NetworkInfo.IsServer && PropSyncable.Cache.TryGet(poolee.gameObject, out var syncable))
                    {
                        PooleeUtilities.SendDespawn(syncable.GetId());
                    }
                }
            }
        }
    }
}
