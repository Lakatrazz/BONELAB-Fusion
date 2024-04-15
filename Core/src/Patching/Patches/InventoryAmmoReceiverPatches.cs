using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoneLib.Nullables;
using HarmonyLib;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.RPC;
using LabFusion.Syncables;
using LabFusion.Utilities;
using SLZ;
using SLZ.Interaction;
using SLZ.Marrow.Data;
using SLZ.Props.Weapons;
using UnityEngine;
using UnityEngine.Timeline;

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

                var inventoryHand = InventoryHand.Cache.Get(hand.gameObject);
                if (inventoryHand)
                {
                    inventoryHand.IgnoreUnlock();
                }

                hand.SetGrabLock();

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
            var data = MagazineClaimData.Create(PlayerIdManager.LocalSmallId, info.syncable.GetId(), handedness);
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
            try
            {
                if (NetworkInfo.HasServer && __instance.rigManager.IsSelf() && Magazine.Cache.Get(host.GetHostGameObject()) && PropSyncable.Cache.TryGet(host.GetHostGameObject(), out var syncable))
                {
                    // Make sure this magazine isn't currently locked in a socket
                    // The base game doesn't check for this and bugs occur in the base game, but due to latency said bugs are more common
                    if (syncable.TryGetExtender<MagazineExtender>(out var extender) && !extender.Component.magazinePlug._isLocked)
                        PooleeUtilities.RequestDespawn(syncable.Id, true);

                    return false;
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("patching InventoryAmmoReceiver.OnHandDrop", e);
#endif
            }

            return true;
        }
    }
}
