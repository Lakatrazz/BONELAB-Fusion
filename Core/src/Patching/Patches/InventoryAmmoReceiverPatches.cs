using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;
using SLZ.Interaction;

namespace LabFusion.Patching {

    [HarmonyPatch(typeof(InventoryAmmoReceiver), nameof(InventoryAmmoReceiver.OnHandGrab))]
    public class InventoryAmmoReceiverPatch {
        public static bool Prefix(InventoryAmmoReceiver __instance, Hand hand) {
            try {
                if (NetworkInfo.HasServer && __instance.rigManager == RigData.RigReferences.RigManager && __instance._selectedMagazineData != null && __instance._selectedMagazineData.spawnable != null) {
                    hand.SetGrabLock();
                    PooleeUtilities.RequestSpawn(__instance._selectedMagazineData.spawnable.crateRef.Barcode, new SerializedTransform(__instance.transform), null, hand.handedness);

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
}
