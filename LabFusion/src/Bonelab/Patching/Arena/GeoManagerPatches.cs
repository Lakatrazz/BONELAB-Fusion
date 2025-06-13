using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Bonelab.Messages;
using LabFusion.Network;
using LabFusion.Scene;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GeoManager))]
public static class GeoManagerPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GeoManager.ToggleGeo))]
    public static bool ToggleGeo(GeoManager __instance, int index)
    {
        if (IgnorePatches)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelHost)
        {
            return false;
        }

        MessageRelay.RelayModule<GeoSelectMessage, GeoSelectData>(new GeoSelectData() { GeoIndex = (byte)index }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GeoManager.ClearCurrentGeo))]
    public static bool ClearCurrentGeo(GeoManager __instance)
    {
        if (IgnorePatches)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelHost)
        {
            return false;
        }

        return true;
    }
}