using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Marrow.Messages;
using LabFusion.Scene;
using LabFusion.Marrow.Extenders;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(InventoryAmmoReceiver))]
public static class InventoryAmmoReceiverPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventoryAmmoReceiver.OnHandGrab))]
    public static bool OnHandGrabPrefix(InventoryAmmoReceiver __instance, Hand hand)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
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
                Spawnable = magazineData.spawnable,
                Position = transform.position,
                Rotation = transform.rotation,
                SpawnCallback = (info) =>
                {
                    OnMagazineSpawned(info, handedness);
                }
            };

            NetworkAssetSpawner.Spawn(info);

            return false;
        }
        catch (Exception e)
        {
            FusionLogger.LogException("patching InventoryAmmoReceiver.OnHandGrab", e);
        }

        return true;
    }

    private static void OnMagazineSpawned(NetworkAssetSpawner.SpawnCallbackInfo info, Handedness handedness)
    {
        var magazine = info.Spawned.GetComponent<Magazine>();
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
        var data = new MagazineClaimData() { OwnerID = PlayerIDManager.LocalSmallID, EntityID = info.Entity.ID, Handedness = handedness };

        MessageRelay.RelayModule<MagazineClaimMessage, MagazineClaimData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventoryAmmoReceiver.OnHandDrop))]
    public static bool OnHandDropPrefix(InventoryAmmoReceiver __instance, IGrippable host)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
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
            NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
            {
                EntityID = entity.ID,
                DespawnEffect = false,
            });

            // Play the ammo release sound effect
            var data = new InventoryAmmoReceiverDropData() { EntityID = PlayerIDManager.LocalID };

            MessageRelay.RelayModule<InventoryAmmoReceiverDropMessage, InventoryAmmoReceiverDropData>(data, CommonMessageRoutes.ReliableToClients);
        }

        return false;
    }
}