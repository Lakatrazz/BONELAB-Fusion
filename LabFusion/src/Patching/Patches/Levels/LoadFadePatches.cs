using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Network;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(FadeAndDespawnVolume._FadeOverTime_d__11))]
public static class FadeAndDespawnVolumePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FadeAndDespawnVolume._FadeOverTime_d__11.MoveNext))]
    public static bool MoveNext()
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        if (NetworkInfo.IsServer)
        {
            return true;
        }

        // If we aren't the server, we don't have load priority
        // So, prevent load fading so we don't get stuck
        return false;
    }
}
