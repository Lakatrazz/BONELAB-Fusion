using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Network;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(Health))]
public static class HealthPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Health.SetAvatar))]
    public static void SetAvatarPostfix(Health __instance, Avatar avatar)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Clear renderers of any null renderers
        // Prevents immortal avatars with damage decals enabled
        var renderers = new List<Renderer>();

        foreach (var renderer in __instance.Renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            renderers.Add(renderer);
        }

        __instance.Renderers = renderers.ToArray();
    }
}
