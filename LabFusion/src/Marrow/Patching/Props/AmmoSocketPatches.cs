using HarmonyLib;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
using LabFusion.Marrow.Messages;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Marrow.Extenders;
using LabFusion.Scene;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(AmmoSocket))]
public static class AmmoSocketPatches
{
    public static bool IgnorePatch { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(AmmoSocket.EjectMagazine))]
    public static bool EjectMagazine(AmmoSocket __instance)
    {
        if (IgnorePatch)
        {
            return true;
        }

        // If the gun is being held by another player, don't locally allow EjectMagazine
        var gun = __instance.gun;
        if (NetworkSceneManager.IsLevelNetworked && gun != null && gun.triggerGrip != null)
        {
            var hand = gun.triggerGrip.GetHand();

            if (hand != null && NetworkPlayerManager.HasExternalPlayer(hand.manager))
            {
                return false;
            }
        }

        return true;
    }

    [HarmonyPatch(nameof(AmmoSocket.OnPlugLocked))]
    [HarmonyPrefix]
    public static void OnPlugLocked(AmmoSocket __instance, Plug plug)
    {
        if (IgnorePatch)
        {
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (!__instance.gun)
        {
            return;
        }

        var gunEntity = GunExtender.Cache.Get(__instance.gun);

        if (gunEntity == null)
        {
            return;
        }

        var ammoPlug = plug.TryCast<AmmoPlug>();

        if (ammoPlug == null || ammoPlug.magazine == null)
        {
            return;
        }

        var magEntity = MagazineExtender.Cache.Get(ammoPlug.magazine);

        if (magEntity == null)
        {
            return;
        }

        var data = new MagazineInsertData() { MagazineID = magEntity.ID, GunID = gunEntity.ID };

        MessageRelay.RelayModule<MagazineInsertMessage, MagazineInsertData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }

    [HarmonyPatch(nameof(AmmoSocket.OnPlugUnlocked))]
    [HarmonyPrefix]
    public static void OnPlugUnlocked(AmmoSocket __instance)
    {
        if (IgnorePatch)
        {
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (!__instance.gun)
        {
            return;
        }

        if (__instance.IsClearOnInsert)
        {
            return;
        }

        var gunEntity = GunExtender.Cache.Get(__instance.gun);

        if (gunEntity == null)
        {
            return;
        }

        var ammoPlug = __instance._magazinePlug;

        if (!ammoPlug || !ammoPlug.magazine)
        {
            return;
        }

        var magEntity = MagazineExtender.Cache.Get(ammoPlug.magazine);

        if (magEntity == null)
        {
            return;
        }

        Hand hand = ammoPlug.host.GetHand(0);
        Handedness handedness = hand != null ? hand.handedness : Handedness.UNDEFINED;

        var data = new MagazineEjectData() { PlayerID = PlayerIDManager.LocalSmallID, MagazineID = magEntity.ID, GunID = gunEntity.ID, Handedness = handedness };

        MessageRelay.RelayModule<MagazineEjectMessage, MagazineEjectData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }
}