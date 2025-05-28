using HarmonyLib;

using LabFusion.Scene;

using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(StabSlash))]
public static class StabSlashPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(StabSlash.ProcessCollision))]
    public static bool ProcessCollision(StabSlash __instance, Collision c, bool isEnter = true)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        var host = __instance._host;

        if (host == null)
        {
            return true;
        }

        var properties = ImpactProperties.Cache.Get(c.gameObject);

        if (properties)
        {
            bool valid = ImpactAttackValidator.ValidateImpact(__instance.gameObject, host, properties);

            return valid;
        }

        return true;
    }
}