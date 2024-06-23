using HarmonyLib;
using Il2CppSLZ.Bonelab;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(TimeManager))]
public static class TimeManagerPatches
{
    public static bool IgnorePatches = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeManager.DECREASE_TIMESCALE))]
    public static bool DECREASE_TIMESCALE(TimeManager __instance)
    {
        if (IgnorePatches)
            return true;

        if (NetworkInfo.HasServer)
        {
            var mode = FusionPreferences.TimeScaleMode;

            switch (mode)
            {
                case TimeScaleMode.DISABLED:
                    return false;
                case TimeScaleMode.CLIENT_SIDE_UNSTABLE:
                    return true;
                case TimeScaleMode.EVERYONE:
                    TimeScaleSender.SendSlowMoButton(true);
                    break;
                case TimeScaleMode.HOST_ONLY:
                    if (NetworkInfo.IsServer)
                        TimeScaleSender.SendSlowMoButton(true);
                    break;
            }
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeManager.TOGGLE_TIMESCALE))]
    public static void TOGGLE_TIMESCALE(TimeManager __instance)
    {
        if (IgnorePatches)
            return;

        if (NetworkInfo.HasServer)
        {
            var mode = FusionPreferences.TimeScaleMode;

            switch (mode)
            {
                case TimeScaleMode.EVERYONE:
                    TimeScaleSender.SendSlowMoButton(false);
                    break;
                case TimeScaleMode.HOST_ONLY:
                    if (NetworkInfo.IsServer)
                        TimeScaleSender.SendSlowMoButton(false);
                    break;
            }
        }
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeManager.SET_TIMESCALE))]
    public static void SET_TIMESCALE(TimeManager __instance, ref float intensity)
    {
        if (IgnorePatches)
            return;

        if (NetworkInfo.HasServer)
        {
            var mode = FusionPreferences.TimeScaleMode;

            switch (mode)
            {
                case TimeScaleMode.LOW_GRAVITY:
                case TimeScaleMode.DISABLED:
                    intensity = 1f;
                    break;
            }
        }
    }
}