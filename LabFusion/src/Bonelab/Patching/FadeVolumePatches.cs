using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Network;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(FadeVolume))]
public static class FadeVolumePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FadeVolume.Awake))]
    public static void Awake(FadeVolume __instance)
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Make sure we aren't the server
        if (NetworkInfo.IsHost)
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
