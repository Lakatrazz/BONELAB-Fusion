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
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.Start))]
        public static void StartPrefix() {
            IgnorePatches = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PullCordDevice.Start))]
        public static void StartPostfix() {
            IgnorePatches = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.EnableBall))]
        public static bool EnableBall(PullCordDevice __instance) {
            // Check the ball joint since SLZ doesn't do this
            if (__instance.ballJoint)
                return false;

            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (PlayerRep.Managers.ContainsKey(__instance.rm))
                    return false;
                else if (__instance.rm == RigData.RigReferences.RigManager) {
                    PlayerSender.SendBodyLogEnable(true);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.DisableBall))]
        public static bool DisableBall(PullCordDevice __instance)
        {
            // Check the ball joint since SLZ doesn't do this
            if (!__instance.ballJoint)
                return false;

            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (PlayerRep.Managers.ContainsKey(__instance.rm))
                    return false;
                else if (__instance.rm == RigData.RigReferences.RigManager) {
                    PlayerSender.SendBodyLogEnable(false);
                }
            }

            return true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.Update))]
        public static void Update(PullCordDevice __instance) {
            // If this is a player rep,
            // We need to disable the avatars inside the body log
            // This way, the player reps won't accidentally change their avatar
            if (NetworkInfo.HasServer && PlayerRep.Managers.ContainsKey(__instance.rm)) {
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
