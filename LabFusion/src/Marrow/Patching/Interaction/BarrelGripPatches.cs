using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Scene;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(BarrelGrip))]
public static class BarrelGripPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BarrelGrip.Awake))]
    public static void AwakePrefix(BarrelGrip __instance)
    {
        OverrideGripSettings(__instance);
    }

    private static void OverrideGripSettings(BarrelGrip grip)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        grip.capMaxBreakForce = float.PositiveInfinity;
        grip.capMinBreakForce = float.PositiveInfinity;

        grip.edgeMaxBreakForce = float.PositiveInfinity;
        grip.edgeMinBreakForce = float.PositiveInfinity;

        grip.ringMaxBreakForce = float.PositiveInfinity;
        grip.ringMinBreakForce = float.PositiveInfinity;

        grip.sideMaxBreakForce = float.PositiveInfinity;
        grip.sideMinBreakForce = float.PositiveInfinity;
    }
}
