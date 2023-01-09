using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Network;
using LabFusion.Utilities;

using SLZ.Bonelab;
using SLZ.Marrow.Warehouse;
using SLZ.SaveData;

using UnityEngine;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(BonelabProgressionHelper))]
    public static class BonelabProgressionHelperPatches
    {
        [HarmonyPatch(nameof(BonelabProgressionHelper.RestoreInventory))]
        [HarmonyPrefix]
        public static bool RestoreInventory(PlayerProgression progression, string levelKey, bool freshLoad, Transform leftHand, Transform rightHand, Func<Barcode, Barcode> itemFilter, string[] priorLevels) {
            // Temporary fix
            // Eventually replace with syncing spawned inventory
            if (NetworkInfo.HasServer)
                return false;

            return true;
        }
    }
}
