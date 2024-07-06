using HarmonyLib;

using LabFusion.Network;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(BonelabProgressionHelper.__restoreSlotsOnReady_d__13))]
public static class BonelabProgressionHelperPatches
{
    [HarmonyPatch(nameof(BonelabProgressionHelper.__restoreSlotsOnReady_d__13.MoveNext))]
    [HarmonyPrefix]
    public static bool RestoreSlotsOnReady()
    {
        // Temporary fix
        // Eventually replace with syncing spawned inventory
        if (NetworkInfo.HasServer)
            return false;

        return true;
    }
}