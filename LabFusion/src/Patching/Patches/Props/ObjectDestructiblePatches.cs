using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.VFX;

using UnityEngine;
using UnityEngine.Events;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ObjectDestructible))]
    public static class ObjectDestructiblePatches
    {
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

                using var message = FusionMessage.Create(NativeMessageTag.ObjectDestructableDestroy, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);
            }
        }

        [HarmonyPatch(nameof(ObjectDestructible.TakeDamage))]
        [HarmonyPrefix]
        public static bool TakeDamagePrefix(ObjectDestructible __instance, Vector3 normal, float damage, bool crit, AttackType attackType)
        {
            if (IgnorePatches)
                return true;

            if (!NetworkInfo.HasServer)
            {
                return true;
            }

            if (ObjectDestructibleExtender.Cache.TryGet(__instance, out var entity) && !entity.IsOwner)
            {
                return false;
            }

            PooleeDespawnPatch.IgnorePatch = true;
            return true;
        }

        [HarmonyPatch(nameof(ObjectDestructible.TakeDamage))]
        [HarmonyPostfix]
        public static void TakeDamagePostfix(ObjectDestructible __instance, Vector3 normal, float damage, bool crit, AttackType attackType)
        {
            PooleeDespawnPatch.IgnorePatch = false;
        }
    }
}
