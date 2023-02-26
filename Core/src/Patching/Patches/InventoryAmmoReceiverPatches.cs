using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Syncables;
using LabFusion.Utilities;

using SLZ.Interaction;
using SLZ.Props.Weapons;

namespace LabFusion.Patching {

    [HarmonyPatch(typeof(InventoryAmmoReceiver), nameof(InventoryAmmoReceiver.OnHandGrab))]
    public class InventoryAmmoReceiverGrab {
        public static bool Prefix(InventoryAmmoReceiver __instance, Hand hand) {
            try {
                if (NetworkInfo.HasServer && __instance.rigManager == RigData.RigReferences.RigManager) {
                    var magazineData = __instance._selectedMagazineData;

                    if (magazineData == null)
                        return false;
                    
                    var cartridgeData = __instance._selectedCartridgeData;
                    
                    if (cartridgeData == null || __instance._AmmoInventory.GetCartridgeCount(cartridgeData) <= 0)
                        return false;

                    var inventoryHand = InventoryHand.Cache.Get(hand.gameObject);
                    if (inventoryHand) {
                        inventoryHand.IgnoreUnlock();
                    }

                    hand.SetGrabLock();
                    PooleeUtilities.RequestSpawn(magazineData.spawnable.crateRef.Barcode, new SerializedTransform(__instance.transform), null, hand.handedness);

                    return false;
                }
            }
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("patching InventoryAmmoReceiver.OnHandGrab", e);
#endif
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(InventoryAmmoReceiver), nameof(InventoryAmmoReceiver.OnHandDrop))]
    public class InventoryAmmoReceiverDrop
    {
        public static bool Prefix(InventoryAmmoReceiver __instance, IGrippable host)
        {
            try
            {
                if (NetworkInfo.HasServer && __instance.rigManager == RigData.RigReferences.RigManager && Magazine.Cache.Get(host.GetHostGameObject()) && PropSyncable.Cache.TryGet(host.GetHostGameObject(), out var syncable)) {
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
