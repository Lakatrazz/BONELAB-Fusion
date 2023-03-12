using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;
using SLZ.Marrow.Data;
using SLZ.Props;

using UnityEngine;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(ObjectDestructable))]
    public static class ObjectDestructablePatches {
        public static bool IgnorePatches = false;

        [HarmonyPatch(nameof(ObjectDestructable.TakeDamage))]
        [HarmonyPrefix]
        public static bool TakeDamagePrefix(ObjectDestructable __instance, Vector3 normal, float damage, bool crit, AttackType attackType, ref bool __state) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && ObjectDestructableExtender.Cache.TryGet(__instance, out var syncable) && !syncable.IsOwner())
                return false;

            __state = __instance._isDead;

            AssetPooleePatches.IgnorePatches = true;
            return true;
        }

        [HarmonyPatch(nameof(ObjectDestructable.TakeDamage))]
        [HarmonyPostfix]
        public static void TakeDamagePostfix(ObjectDestructable __instance, Vector3 normal, float damage, bool crit, AttackType attackType, ref bool __state) {
            AssetPooleePatches.IgnorePatches = false;

            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer && ObjectDestructableExtender.Cache.TryGet(__instance, out var syncable) && syncable.TryGetExtender<ObjectDestructableExtender>(out var extender)) {
                // Send object destroy
                if (syncable.IsOwner() && !__state && __instance._isDead) {
                    using (var writer = FusionWriter.Create(ObjectDestructableDestroyData.Size))
                    {
                        using (var data = ObjectDestructableDestroyData.Create(PlayerIdManager.LocalSmallId, syncable.Id, extender.GetIndex(__instance).Value))
                        {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.ObjectDestructableDestroy, writer))
                            {
                                MessageSender.SendToServer(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
        }
    }
}
