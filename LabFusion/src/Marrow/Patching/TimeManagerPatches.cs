using HarmonyLib;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Utilities;

using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;

using UnityEngine;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(TimeManager))]
public static class TimeManagerPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeManager.DECREASE_TIMESCALE))]
    public static bool DECREASE_TIMESCALE()
    {
        if (IgnorePatches)
        {
            return true;
        }

        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        var mode = CommonPreferences.SlowMoMode;

        switch (mode)
        {
            case TimeScaleMode.LOW_GRAVITY:

                int step = Mathf.Min(TimeManager.CurrentTimeScaleStep + 1, TimeManager.max_timeScaleStep);
                TimeManager.cur_timeScaleStep = step;
                TimeManager.cur_intensity = Mathf.Pow(2f, step);

                ResetTimeScale();

                return false;
            case TimeScaleMode.EVERYONE:
                TimeScaleSender.SendSlowMoButton(true);
                break;
            case TimeScaleMode.HOST_ONLY:
                if (NetworkInfo.IsHost)
                {
                    TimeScaleSender.SendSlowMoButton(true);
                }
                else
                {
                    return false;
                }
                break;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeManager.TOGGLE_TIMESCALE))]
    public static bool TOGGLE_TIMESCALE()
    {
        if (IgnorePatches)
        {
            return true;
        }

        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        var mode = CommonPreferences.SlowMoMode;

        switch (mode)
        {
            case TimeScaleMode.EVERYONE:
                TimeScaleSender.SendSlowMoButton(false);
                break;
            case TimeScaleMode.HOST_ONLY:
                if (NetworkInfo.IsHost)
                {
                    TimeScaleSender.SendSlowMoButton(false);
                }
                else
                {
                    return false;
                }
                break;
            case TimeScaleMode.LOW_GRAVITY:
                TimeManager.cur_intensity = 1f;

                ResetTimeScale();
                break;
        }

        return true;
    }

    private static void ResetTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 1f / MarrowGame.xr.Display.GetRecommendedPhysFrequency();
    }
}