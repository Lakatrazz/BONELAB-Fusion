using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;

using SLZ.Interaction;
using SLZ.Props;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(FlyingGun))]
    public static class FlyingGunPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FlyingGun.OnTriggerGripUpdate))]
        public static void OnTriggerGripUpdate(FlyingGun __instance, Hand hand, ref bool __state)
        {
            __state = __instance._noClipping;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(FlyingGun.OnTriggerGripUpdate))]
        public static void OnTriggerGripUpdatePostfix(FlyingGun __instance, Hand hand, bool __state) {
            if (NetworkInfo.HasServer && hand.manager == RigData.RigReferences.RigManager && __state != __instance._noClipping && NimbusGunExtender.Cache.TryGet(__instance, out var syncable)) {
                using (var writer = FusionWriter.Create()) {
                    using (var data = NimbusGunNoclipData.Create(PlayerIdManager.LocalSmallId, syncable.GetId(), __instance._noClipping)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.NimbusGunNoclip, writer)) {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
