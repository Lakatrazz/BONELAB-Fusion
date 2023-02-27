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
        public static bool OnTriggerGripUpdate(FlyingGun __instance, Hand hand, ref bool __state) {
            // In a server, prevent two nimbus guns from sending you flying out of the map
            // Due to SLZ running these forces on update for whatever reason, the forces are inconsistent
            if (NetworkInfo.HasServer && hand.handedness == SLZ.Handedness.LEFT) {
                var otherHand = hand.otherHand;
                
                if (otherHand.m_CurrentAttachedGO) {
                    var otherGrip = Grip.Cache.Get(otherHand.m_CurrentAttachedGO);
                    
                    if (otherGrip.HasHost) {
                        var host = otherGrip.Host.GetHostGameObject();

                        if (host.GetComponent<FlyingGun>() != null)
                            return false;
                    }
                }
            }

            __state = __instance._noClipping;

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(FlyingGun.OnTriggerGripUpdate))]
        public static void OnTriggerGripUpdatePostfix(FlyingGun __instance, Hand hand, bool __state) {
            if (NetworkInfo.HasServer && hand.manager == RigData.RigReferences.RigManager && __state != __instance._noClipping && FlyingGunExtender.Cache.TryGet(__instance, out var syncable)) {
                using (var writer = FusionWriter.Create(NimbusGunNoclipData.Size)) {
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
