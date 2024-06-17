using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.RPC;
using LabFusion.Syncables;
using LabFusion.Utilities;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;


namespace LabFusion.Patching
{

    [HarmonyPatch(typeof(InventoryAmmoReceiver), nameof(InventoryAmmoReceiver.OnHandGrab))]
    public class InventoryAmmoReceiverGrab
    {
        public static bool Prefix(InventoryAmmoReceiver __instance, Hand hand)
        {
            if (!NetworkInfo.HasServer)
            {
                return true;
            }

            if (!__instance.rigManager.IsSelf())
            {
                return true;
            }

            try
            {
                var magazineData = __instance._selectedMagazineData;

                if (magazineData == null)
                    return false;

                var cartridgeData = __instance._selectedCartridgeData;

                if (cartridgeData == null || __instance._AmmoInventory.GetCartridgeCount(cartridgeData) <= 0)
                    return false;

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

            MagazineUtilities.GrabMagazine(magazine, RigData.RigReferences.RigManager, handedness);

            // Send claim message
            using var writer = FusionWriter.Create(MagazineClaimData.Size);
            var data = MagazineClaimData.Create(PlayerIdManager.LocalSmallId, info.entity.Id, handedness);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.MagazineClaim, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
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

            if (!__instance.rigManager.IsSelf())
            {
                return true;
            }

            var interactableHost = host.TryCast<InteractableHost>();

            if (interactableHost == null)
            {
                return true;
            }

            if (!InteractableHostExtender.Cache.TryGet(interactableHost, out var entity))
            {
                return true;
            }

            var magazineExtender = entity.GetExtender<Entities.MagazineExtender>();

            if (magazineExtender == null)
            {
                return true;
            }

            // Make sure this magazine isn't currently locked in a socket
            // The base game doesn't check for this and bugs occur in the base game, but due to latency said bugs are more common
            if (!magazineExtender.Component.magazinePlug._isLocked)
            {
                PooleeUtilities.RequestDespawn(entity.Id, true);
            }

            return false;
        }
    }
}
