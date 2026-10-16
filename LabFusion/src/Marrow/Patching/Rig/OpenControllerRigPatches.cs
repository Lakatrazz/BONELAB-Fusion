using HarmonyLib;

using Il2CppSLZ.Marrow.Input;
using Il2CppSLZ.Marrow;

using UnityEngine;

using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Utilities;

using UnityEngine.Rendering;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(XRHMD))]
public static class XRHMDPatches
{
    [HarmonyPatch(nameof(XRHMD.IsUserPresent), MethodType.Getter)]
    [HarmonyPostfix]
    public static void IsUserPresent(ref bool __result)
    {
        if (!NetworkManager.HasServer)
        {
            return;
        }

        // While in a server, prevent the game from pausing
        __result = true;
    }
}

[HarmonyPatch(typeof(OpenControllerRig))]
public static class OpenControllerRigPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(OpenControllerRig.OnBeginCameraRendering))]
    public static bool OnBeginCameraRenderingPrefix(OpenControllerRig __instance, ScriptableRenderContext ctx, Camera cam)
    {
        if (NetworkPlayerManager.HasExternalPlayer(__instance.manager))
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(OpenControllerRig.OnEarlyUpdate))]
    public static void OnEarlyUpdatePrefix(OpenControllerRig __instance, ref Il2CppSystem.Action<bool> __state)
    {
        __state = OpenControllerRig.OnPauseStateChange;

        // If this isn't our player, the pause callback should not be called
        if (!__instance.manager.IsLocalPlayer())
        {
            OpenControllerRig.OnPauseStateChange = null;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(OpenControllerRig.OnEarlyUpdate))]
    public static void OnEarlyUpdatePostfix(ref Il2CppSystem.Action<bool> __state)
    {
        OpenControllerRig.OnPauseStateChange = __state;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(OpenControllerRig.OnRealHeptaEarlyUpdate))]
    public static void OnRealHeptaEarlyUpdatePrefix(OpenControllerRig __instance)
    {
        var rigManager = __instance.manager;

        if (NetworkRig.Cache.TryGet(rigManager, out var networkRig) && !networkRig.NetworkEntity.IsOwner)
        {
            networkRig.OnOverrideControllerRig();
        }
    }
}
