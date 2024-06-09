using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.VFX;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ObjectDestructible))]
    public static class ObjectDestructablePatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPatch(nameof(ObjectDestructible.TakeDamage))]
        [HarmonyPrefix]
        public static bool TakeDamagePrefix(ObjectDestructible __instance, Vector3 normal, float damage, bool crit, AttackType attackType, ref bool __state)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && ObjectDestructableExtender.Cache.TryGet(__instance, out var syncable) && !syncable.IsOwner())
                return false;

            __state = __instance._isDead;

            PooleeDespawnPatch.IgnorePatch = true;
            return true;
        }

        [HarmonyPatch(nameof(ObjectDestructible.TakeDamage))]
        [HarmonyPostfix]
        public static void TakeDamagePostfix(ObjectDestructible __instance, Vector3 normal, float damage, bool crit, AttackType attackType, ref bool __state)
        {
            PooleeDespawnPatch.IgnorePatch = false;

            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer && ObjectDestructableExtender.Cache.TryGet(__instance, out var syncable) && syncable.TryGetExtender<ObjectDestructableExtender>(out var extender))
            {
                // Send object destroy
                if (syncable.IsOwner() && !__state && __instance._isDead)
                {
                    using var writer = FusionWriter.Create(ComponentIndexData.Size);
                    var data = ComponentIndexData.Create(PlayerIdManager.LocalSmallId, syncable.Id, extender.GetIndex(__instance).Value);
                    writer.Write(data);

                    using var message = FusionMessage.Create(NativeMessageTag.ObjectDestructableDestroy, writer);
                    MessageSender.SendToServer(NetworkChannel.Reliable, message);
                }
            }
        }
    }
}
