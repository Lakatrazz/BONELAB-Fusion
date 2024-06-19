using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Bonelab;
using LabFusion.Entities;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(PullCordDevice))]
    public static class PullCordDevicePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.Update))]
        public static void Update(PullCordDevice __instance)
        {
            // If this is a player rep,
            // We need to disable the avatars inside the body log
            // This way, the player reps won't accidentally change their avatar
            if (NetworkInfo.HasServer && NetworkPlayerManager.HasExternalPlayer(__instance.rm))
            {
                for (var i = 0; i < __instance.avatarCrateRefs.Length; i++)
                {
                    __instance.avatarCrateRefs[i].Barcode = (Barcode)"";
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.EnableBall))]
        public static void EnableBall(PullCordDevice __instance)
        {
            if (NetworkInfo.HasServer && __instance.rm.IsSelf())
            {
                PullCordSender.SendBodyLogToggle(true);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.DisableBall))]
        public static void DisableBall(PullCordDevice __instance)
        {
            if (NetworkInfo.HasServer && __instance.rm.IsSelf())
            {
                PullCordSender.SendBodyLogToggle(false);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.PlayAvatarParticleEffects))]
        public static void PlayAvatarParticleEffects(PullCordDevice __instance)
        {
            if (NetworkInfo.HasServer && __instance.rm.IsSelf())
            {
                PullCordSender.SendBodyLogEffect();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordDevice.OnBallGripDetached))]
        public static void OnBallGripDetached(PullCordDevice __instance, Hand hand)
        {
            // Prevent player rep body logs from inserting into the body mall
            if (NetworkInfo.HasServer && __instance.rm != RigData.RigReferences.RigManager)
            {
                var apv = __instance.apv;

                if (apv != null && apv.bodyLog == __instance)
                {
                    apv.bodyLog = null;
                }

                __instance.apv = null;
                __instance.isHandleInReceiver = false;
                __instance.isBallInReceiver = false;
            }
        }
    }
}
