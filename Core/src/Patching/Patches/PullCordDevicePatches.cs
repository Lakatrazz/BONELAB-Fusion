using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;

using SLZ.Props;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(PullCordDevice))]
    public static class PullCordDevicePatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.PlayAvatarParticleEffects))]
        public static void PlayAvatarParticleEffects(PullCordDevice __instance) {
            if (NetworkInfo.HasServer && __instance.rm == RigData.RigReferences.RigManager) {
                using (var writer = FusionWriter.Create()) {
                    using (var data = BodyLogEffectData.Create(PlayerIdManager.LocalSmallId)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.BodyLogEffect, writer)) {
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
