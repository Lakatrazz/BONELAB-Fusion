using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using SLZ.Marrow.Data;
using SLZ.Props;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(Prop_Health))]
    public static class PropHealthPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Prop_Health.TAKEDAMAGE))]
        public static bool TAKEDAMAGE(Prop_Health __instance, float damage, bool crit = false, AttackType attackType = AttackType.None)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && PropHealthExtender.Cache.TryGet(__instance, out var syncable) && !syncable.IsOwner())
                return false;

            return true;
        }



        [HarmonyPrefix]
        [HarmonyPatch(nameof(Prop_Health.DESTROYED))]
        public static bool DESTROYEDPrefix(Prop_Health __instance) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && PropHealthExtender.Cache.TryGet(__instance, out var syncable) && syncable.TryGetExtender<PropHealthExtender>(out var extender)) {
                if (!syncable.IsOwner())
                    return false;
                // Send object destroy
                else {
                    using (var writer = FusionWriter.Create(PropHealthDestroyData.Size)) {
                        using (var data = PropHealthDestroyData.Create(PlayerIdManager.LocalSmallId, syncable.Id, extender.GetIndex(__instance).Value)) {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PropHealthDestroy, writer)) {
                                MessageSender.SendToServer(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                    return true;
                }
            }

            AssetPooleePatches.IgnorePatches = true;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Prop_Health.DESTROYED))]
        public static void DESTROYEDPostfix(Prop_Health __instance) {
            AssetPooleePatches.IgnorePatches = false;
        }
    }
}
