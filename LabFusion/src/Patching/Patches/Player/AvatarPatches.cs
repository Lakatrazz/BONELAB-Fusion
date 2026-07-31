using HarmonyLib;

using LabFusion.Network;
using LabFusion.Marrow.Extensions;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(Avatar))]
public static class AvatarPatches
{
    [HarmonyPatch(nameof(Avatar.Awake))]
    [HarmonyPrefix]
    public static void AwakePrefix(Avatar __instance)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Avatar SurfaceDataCards currently don't get loaded properly, base game avatars set the inaccessible asset reference
        // If this is changed in a future patch this can be removed
        __instance.LoadSurfaceData();
    }
}