using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Network;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(FadeAndDespawnVolume))]
public static class FadeAndDespawnVolumePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FadeAndDespawnVolume.Awake))]
    public static void Awake(FadeAndDespawnVolume __instance)
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Make sure we aren't the server
        if (NetworkInfo.IsServer)
        {
            return;
        }

        // If we aren't the server, we don't have load priority
        // So, we'd get stuck in a load fade
        // By disabling these components, we prevent the fade from actually visibly showing
        __instance.volume.enabled = false;
        __instance.meshRenderer.enabled = false;
    }
}
