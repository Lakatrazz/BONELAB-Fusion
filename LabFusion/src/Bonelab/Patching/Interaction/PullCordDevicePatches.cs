using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Bonelab.Messages;
using LabFusion.Player;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(PullCordDevice))]
public static class PullCordDevicePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordDevice.Update))]
    public static void Update(PullCordDevice __instance)
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // If this is a networked player,
        // We need to disable the avatars inside the body log
        // This way, the player reps won't accidentally change their avatar
        if (NetworkPlayerManager.HasExternalPlayer(__instance.rm))
        {
            for (var i = 0; i < __instance.avatarCrateRefs.Length; i++)
            {
                __instance.avatarCrateRefs[i].Barcode = Barcode.EmptyBarcode();
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordDevice.EnableBall))]
    public static void EnableBall(PullCordDevice __instance)
    {
        if (NetworkInfo.HasServer && __instance.rm.IsLocalPlayer())
        {
            MessageRelay.RelayModule<BodyLogToggleMessage, BodyLogToggleData>(new() { PlayerID = PlayerIDManager.LocalSmallID, IsEnabled = true, }, CommonMessageRoutes.ReliableToOtherClients);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordDevice.DisableBall))]
    public static void DisableBall(PullCordDevice __instance)
    {
        if (NetworkInfo.HasServer && __instance.rm.IsLocalPlayer())
        {
            MessageRelay.RelayModule<BodyLogToggleMessage, BodyLogToggleData>(new() { PlayerID = PlayerIDManager.LocalSmallID, IsEnabled = false, }, CommonMessageRoutes.ReliableToOtherClients);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordDevice.PlayAvatarParticleEffects))]
    public static void PlayAvatarParticleEffects(PullCordDevice __instance)
    {
        if (NetworkInfo.HasServer && __instance.rm.IsLocalPlayer())
        {
            MessageRelay.RelayModule<BodyLogEffectMessage, BodyLogEffectData>(new() { PlayerID = PlayerIDManager.LocalSmallID }, CommonMessageRoutes.UnreliableToOtherClients);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordDevice.OnBallGripDetached))]
    public static void OnBallGripDetached(PullCordDevice __instance, Hand hand)
    {
        // Prevent player rep body logs from inserting into the body mall
        if (NetworkInfo.HasServer && __instance.rm != RigData.Refs.RigManager)
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