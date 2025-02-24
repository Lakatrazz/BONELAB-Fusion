using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(InventoryAmmoReceiver), nameof(InventoryAmmoReceiver.OnHandGrab))]
public class InventoryAmmoReceiverGrab
{
    public static bool Prefix(InventoryAmmoReceiver __instance, Hand hand)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        if (!__instance._parentRigManager.IsLocalPlayer())
        {
            return true;
        }

        if (LocalControls.DisableAmmoPouch || LocalControls.DisableInteraction)
        {
            return false;
        }

        try
        {
            var magazineData = __instance._selectedMagazineData;

            if (magazineData == null)
            {
                return false;
            }

            var cartridgeData = __instance._selectedCartridgeData;

            if (cartridgeData == null || AmmoInventory.Instance.GetCartridgeCount(cartridgeData) <= 0)
            {
                return false;
            }

            Handedness handedness = hand.handedness;

            var transform = __instance.transform;
            var info = new NetworkAssetSpawner.SpawnRequestInfo()
            {
                spawnable = magazineData.spawnable,
                position = transform.position,
                rotation = transform.rotation,
                spawnCallback = (info) =>
                {
                    OnMagazineSpawned(info, handedness);
                }
            };

            NetworkAssetSpawner.Spawn(info);

            return false;
        }
        catch (Exception e)
        {
#if DEBUG
            FusionLogger.LogException("patching InventoryAmmoReceiver.OnHandGrab", e);
#endif
        }

        return true;
    }

    private static void OnMagazineSpawned(NetworkAssetSpawner.SpawnCallbackInfo info, Handedness handedness)
    {
        var magazine = info.spawned.GetComponent<Magazine>();
        if (magazine == null)
        {
            return;
        }

        var localPlayer = LocalPlayer.GetNetworkPlayer();

        if (localPlayer != null)
        {
            MagazineUtilities.GrabMagazine(magazine, localPlayer, handedness);
        }

        // Send claim message
        var data = MagazineClaimData.Create(PlayerIdManager.LocalSmallId, info.entity.Id, handedness);

        MessageRelay.RelayNative(data, NativeMessageTag.MagazineClaim, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}

[HarmonyPatch(typeof(InventoryAmmoReceiver), nameof(InventoryAmmoReceiver.OnHandDrop))]
public class InventoryAmmoReceiverDrop
{
    public static bool Prefix(InventoryAmmoReceiver __instance, IGrippable host)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        if (!__instance._parentRigManager.IsLocalPlayer())
        {
            return true;
        }

        var interactableHost = host.TryCast<InteractableHost>();

        if (interactableHost == null)
        {
            return true;
        }

        if (!InteractableHostExtender.Cache.TryGet(interactableHost, out var entity) || !entity.IsRegistered)
        {
            return true;
        }

        var magazineExtender = entity.GetExtender<MagazineExtender>();

        if (magazineExtender == null)
        {
            return true;
        }

        // Make sure this magazine isn't currently locked in a socket
        // The base game doesn't check for this and bugs occur in the base game (as of patch #3), but due to latency said bugs are more common
        if (!magazineExtender.Component.magazinePlug._isLocked)
        {
            // Despawn the magazine
            PooleeUtilities.RequestDespawn(entity.Id, false);

            // Play the ammo release sound effect
            var data = InventoryAmmoReceiverDropData.Create(PlayerIdManager.LocalId);

            MessageRelay.RelayNative(data, NativeMessageTag.InventoryAmmoReceiverDrop, NetworkChannel.Reliable, RelayType.ToClients);
        }

        return false;
    }
}