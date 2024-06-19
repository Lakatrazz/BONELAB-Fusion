using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Representation;
using LabFusion.SDK.Achievements;
using LabFusion.Entities;
using LabFusion.RPC;
using LabFusion.Marrow;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;

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

            bool isPlayerRep = NetworkPlayerManager.HasExternalPlayer(manager);

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
            {
                return;
            }

            if (!NetworkInfo.HasServer)
            {
                return;
            }

            var gunEntity = GunExtender.Cache.Get(__instance);

            if (gunEntity == null)
            {
                return;
            }

            var gunExtender = gunEntity.GetExtender<GunExtender>();

            try
            {
                // Make sure this is being grabbed by our main player
                if (__instance.triggerGrip && __instance.triggerGrip.attachedHands.Find((Il2CppSystem.Predicate<Hand>)((h) => h.manager.IsSelf())))
                {
                    using var writer = FusionWriter.Create(GunShotData.Size);
                    var ammoCount = __instance._magState != null ? (byte)__instance._magState.AmmoCount : (byte)0;

                    var data = GunShotData.Create(PlayerIdManager.LocalSmallId, ammoCount, gunEntity.Id, (byte)gunExtender.GetIndex(__instance).Value);
                    writer.Write(data);

                    using var message = FusionMessage.Create(NativeMessageTag.GunShot, writer);
                    MessageSender.SendToServer(NetworkChannel.Reliable, message);
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
            {
                return;
            }

            if (!NetworkInfo.HasServer)
            {
                return;
            }

            if (__instance._selectedCrate == null)
            {
                return;
            }

            var entity = SpawnGunExtender.Cache.Get(__instance);

            if (entity == null || !entity.IsOwner)
            {
                return;
            }

            string barcode = __instance._selectedCrate.Barcode;

            using var writer = FusionWriter.Create(SpawnGunPreviewMeshData.GetSize(barcode));
            var data = SpawnGunPreviewMeshData.Create(PlayerIdManager.LocalSmallId, entity.Id, barcode);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.SpawnGunPreviewMesh, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpawnGun.OnFire))]
        public static bool OnFirePrefix(SpawnGun __instance, ref SpawnableCrate __state)
        {
            if (!NetworkInfo.HasServer)
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
            if (PlayerIdManager.HasOtherPlayers && AchievementManager.TryGetAchievement<CleanupCrew>(out var achievement))
            {
                achievement.IncrementTask();
            }

            // Send a despawn request if there's a syncable
            var poolee = Poolee.Cache.Get(marrowEntity.gameObject);

            if (PooleeExtender.Cache.TryGet(poolee, out var entity))
            {
                PooleeUtilities.RequestDespawn(entity.Id);
            }

            // Flash the spawn gun
            spawnGun.Flash();
            spawnGun.SpawnFlareAsync().Forget();
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
        }
    }
}
