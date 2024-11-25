using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Entities;
using LabFusion.RPC;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow;

using UnityEngine;
using UnityEngine.Events;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ObjectDestructible))]
    public static class ObjectDestructiblePatches
    {
        public class ObjectDestructibleState
        {
            public bool isDead;
            public LootTableData lootTable;

            public Vector3 spawnPosition;
            public Quaternion spawnRotation;

            public bool ignoringPatch = false;

            public ObjectDestructibleState(ObjectDestructible destructible)
            {
                lootTable = destructible.lootTable;
                isDead = destructible._isDead;

                var spawnTarget = destructible.spawnTarget;

                if (spawnTarget == null)
                {
                    spawnTarget = destructible.transform;
                }

                spawnPosition = spawnTarget.position;
                spawnRotation = spawnTarget.rotation;
            }
        }

        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ObjectDestructible.Awake))]
        public static void Awake(ObjectDestructible __instance)
        {
            // The OnDestruction action gets reset when the destructible is despawned
            // But the UnityEvent is never reset, so we can hook into that instead
            void OnDestruct()
            {
                OnDestruction(__instance);
            }
            var destructAction = (UnityAction)OnDestruct;

            __instance.OnDestruct.AddListener(destructAction);
        }

        private static void OnDestruction(ObjectDestructible destructible)
        {
            if (!NetworkInfo.HasServer)
            {
                return;
            }

            var entity = ObjectDestructibleExtender.Cache.Get(destructible);

            if (entity == null)
            {
                return;
            }

            var extender = entity.GetExtender<ObjectDestructibleExtender>();

            // Send object destroy
            if (entity.IsOwner)
            {
                using var writer = FusionWriter.Create(ComponentIndexData.Size);
                var data = ComponentIndexData.Create(PlayerIdManager.LocalSmallId, entity.Id, (byte)extender.GetIndex(destructible).Value);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.ObjectDestructibleDestroy, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);
            }
        }

        [HarmonyPatch(nameof(ObjectDestructible.TakeDamage))]
        [HarmonyPrefix]
        public static bool TakeDamagePrefix(ObjectDestructible __instance, Vector3 normal, float damage, bool crit, AttackType attackType, ref ObjectDestructibleState __state)
        {
            __state = new(__instance);

            // Make sure we have a server
            if (!NetworkInfo.HasServer)
            {
                return true;
            }

            // Clear loot table
            __instance.lootTable = null;

            // Make sure patches aren't being ignored
            if (IgnorePatches)
            {
                return true;
            }

            if (ObjectDestructibleExtender.Cache.TryGet(__instance, out var entity) && !entity.IsOwner)
            {
                __state.ignoringPatch = true;
                return false;
            }

            PooleeDespawnPatch.IgnorePatch = true;
            return true;
        }

        [HarmonyPatch(nameof(ObjectDestructible.TakeDamage))]
        [HarmonyPostfix]
        public static void TakeDamagePostfix(ObjectDestructible __instance, Vector3 normal, float damage, bool crit, AttackType attackType, ref ObjectDestructibleState __state)
        {
            PooleeDespawnPatch.IgnorePatch = false;

            // Make sure we have a server
            if (!NetworkInfo.HasServer)
            {
                return;
            }

            // Reset loot table
            __instance.lootTable = __state.lootTable;

            // Check if patches are being ignored
            if (IgnorePatches)
            {
                return;
            }

            // Check if the prefix was ignored
            if (__state.ignoringPatch)
            {
                return;
            }

            // Check if we need to spawn loot
            bool destroyed = __instance._isDead && !__state.isDead;
            if (destroyed && __instance.lootTable != null)
            {
                var spawnable = __instance.lootTable.GetLootItem();

                NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
                {
                    spawnable = spawnable,
                    position = __state.spawnPosition,
                    rotation = __state.spawnRotation,
                    spawnCallback = (info) =>
                    {
                        __instance.OnLootSpawn?.Invoke(__instance, spawnable, info.spawned);
                    }
                });
            }
        }
    }
}
