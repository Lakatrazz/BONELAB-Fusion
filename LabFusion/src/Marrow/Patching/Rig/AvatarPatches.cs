using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Marrow.Extensions;
using LabFusion.Marrow.Rig;
using LabFusion.Scene;
using LabFusion.Utilities;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(Avatar))]
public static class AvatarPatches
{
    [HarmonyPatch(nameof(Avatar.Awake))]
    [HarmonyPrefix]
    public static void AwakePrefix(Avatar __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        // Avatar SurfaceDataCards currently don't get loaded properly, base game avatars set the inaccessible asset reference
        // If this is changed in a future patch this can be removed
        __instance.LoadSurfaceData();
    }

    [HarmonyPatch(nameof(Avatar.RefreshBodyMeasurements))]
    [HarmonyPatch(new Type[0])]
    [HarmonyPostfix]
    public static void RefreshBodyMeasurementsPostfix(Avatar __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        try
        {
            InvokeNetworkRigCallback(__instance);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("patching Avatar.RefreshBodyMeasurements", e);
        }
    }

    private static void InvokeNetworkRigCallback(Avatar avatar)
    {
        if (!TryGetNetworkRig(avatar, out var networkRig))
        {
            return;
        }

        networkRig.AvatarSetter.OnRefreshBodyMeasurements(avatar);
    }

    private static bool TryGetNetworkRig(Avatar avatar, out NetworkRig networkRig)
    {
        networkRig = null;

        if (avatar.TryCast<RealHeptaAvatar>() != null)
        {
            return false;
        }

        var parent = avatar.transform.parent;

        if (parent == null)
        {
            return false;
        }

        var rigManager = RigManager.Cache.Get(parent.gameObject);

        if (rigManager == null)
        {
            return false;
        }

        return NetworkBeingManager.TryGetNetworkRig(rigManager, out networkRig);
    }
}