using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using SLZ.Marrow.Warehouse;
using SLZ.Props;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(PullCordDevice))]
    public static class PullCordDevicePatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.Update))]
        public static void Update(PullCordDevice __instance) {
            // If this is a player rep,
            // We need to disable the avatars inside the body log
            // This way, the player reps won't accidentally change their avatar
            if (NetworkInfo.HasServer && PlayerRepManager.HasPlayerId(__instance.rm)) {
                for (var i = 0; i < __instance.avatarCrateRefs.Length; i++) {
                    __instance.avatarCrateRefs[i].Barcode = (Barcode)"";
                }
            }
        }

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
