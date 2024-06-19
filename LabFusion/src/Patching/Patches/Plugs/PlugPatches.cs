using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Entities;
using LabFusion.Utilities;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(AlignPlug))]
public static class AlignPlugPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AlignPlug.OnHandAttached))]
    public static bool OnHandAttached(InteractableHost host, Hand hand)
    {
        if (NetworkInfo.HasServer && NetworkPlayerManager.HasExternalPlayer(hand.manager))
            return false;

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(AlignPlug.OnHandDetached))]
    public static bool OnHandDetached(InteractableHost host, Hand hand)
    {
        if (NetworkInfo.HasServer && NetworkPlayerManager.HasExternalPlayer(hand.manager))
            return false;

        return true;
    }

    [HarmonyPatch(nameof(AlignPlug.OnProxyGrab))]
    [HarmonyPrefix]
    public static bool OnProxyGrab(AlignPlug __instance, Hand hand)
    {
        if (NetworkInfo.HasServer && __instance.TryCast<AmmoPlug>() && NetworkPlayerManager.HasExternalPlayer(hand.manager))
            return false;

        return true;
    }

    [HarmonyPatch(nameof(AlignPlug.OnProxyRelease))]
    [HarmonyPrefix]
    public static bool OnProxyRelease(AlignPlug __instance, Hand hand)
    {
        if (NetworkInfo.HasServer && __instance.TryCast<AmmoPlug>() && NetworkPlayerManager.HasExternalPlayer(hand.manager))
            return false;

        return true;
    }
}

[HarmonyPatch(typeof(AmmoPlug))]
public static class AmmoPlugPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AmmoPlug.OnPlugInsertComplete))]
    public static void OnPlugInsertCompletePrefix()
    {
        PooleeDespawnPatch.IgnorePatch = true;
        AmmoSocketPatches.IgnorePatch = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(AmmoPlug.OnPlugInsertComplete))]
    public static void OnPlugInsertCompletePostfix()
    {
        PooleeDespawnPatch.IgnorePatch = false;
        AmmoSocketPatches.IgnorePatch = false;
    }
}

[HarmonyPatch(typeof(AmmoSocket))]
public static class AmmoSocketPatches
{
    public static bool IgnorePatch = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(AmmoSocket.EjectMagazine))]
    public static bool EjectMagazine(AmmoSocket __instance)
    {
        if (IgnorePatch)
            return true;

        // If the gun is being held by another player, don't locally allow EjectMagazine
        var gun = __instance.gun;
        if (NetworkInfo.HasServer && gun != null && gun.triggerGrip != null)
        {
            var hand = gun.triggerGrip.GetHand();

            if (hand != null && NetworkPlayerManager.HasExternalPlayer(hand.manager))
                return false;
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

        if (!NetworkInfo.HasServer)
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

        using var writer = FusionWriter.Create(MagazineInsertData.Size);
        var data = MagazineInsertData.Create(PlayerIdManager.LocalSmallId, magEntity.Id, gunEntity.Id);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.MagazineInsert, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }

    [HarmonyPatch(nameof(AmmoSocket.OnPlugUnlocked))]
    [HarmonyPrefix]
    public static void OnPlugUnlocked(AmmoSocket __instance)
    {
        if (IgnorePatch)
        {
            return;
        }

        if (!NetworkInfo.HasServer)
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

        using var writer = FusionWriter.Create(MagazineEjectData.Size);
        Hand hand = ammoPlug.host.GetHand(0);
        Handedness handedness = hand != null ? hand.handedness : Handedness.UNDEFINED;

        var data = MagazineEjectData.Create(PlayerIdManager.LocalSmallId, magEntity.Id, gunEntity.Id, handedness);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.MagazineEject, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }
}