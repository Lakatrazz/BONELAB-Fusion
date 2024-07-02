using HarmonyLib;
using LabFusion.Network;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Bonelab.SaveData;

using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LabFusion.Utilities;
using Il2CppNewtonsoft.Json.Linq;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(BonelabProgressionHelper.__restoreSlotsOnReady_d__13))]
    public static class BonelabProgressionHelperPatches
    {
        [HarmonyPatch(nameof(BonelabProgressionHelper.__restoreSlotsOnReady_d__13.MoveNext))]
        [HarmonyPrefix]
        public static bool OnSlotsReady()
        {
            // Temporary fix
            // Eventually replace with syncing spawned inventory
            if (NetworkInfo.HasServer)
                return false;

            return true;
        }
    }
}
