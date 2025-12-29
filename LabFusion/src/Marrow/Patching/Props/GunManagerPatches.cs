using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Marrow.Extenders;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(GunManager))]
public static class GunManagerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GunManager.OnGunGrabbed))]
    public static void OnGunGrabbed(Hand hand, Gun gun)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Check if this is a networked player
        // If it is, we need to manually switch the magazine in the ammo pouch
        if (NetworkPlayerManager.TryGetPlayer(hand.manager, out var player) && !player.NetworkEntity.IsOwner)
        {
            OnNetworkGrab(player, gun);
        }
    }

    private static void OnNetworkGrab(NetworkPlayer player, Gun gun)
    {
        var ammoReceiverExtender = player.NetworkEntity.GetExtender<InventoryAmmoReceiverExtender>();

        if (ammoReceiverExtender == null)
        {
            return;
        }

        var ammoReceiver = ammoReceiverExtender.Component;

        // Switch the magazine and cartridge data, so that grabbed magazines have the correct type
        ammoReceiver.SwitchMagazine(gun.defaultMagazine, gun.defaultCartridge);
    }
}