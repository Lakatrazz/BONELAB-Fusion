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
    public static bool IgnorePatches = false;

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
            case TimeScaleMode.DISABLED:
                return false;
            case TimeScaleMode.LOW_GRAVITY:

                int step = Mathf.Min(TimeManager.CurrentTimeScaleStep + 1, TimeManager.max_timeScaleStep);
                TimeManager.cur_timeScaleStep = step;
                TimeManager.cur_intensity = Mathf.Pow(2f, step);

                Time.timeScale = 1f;
                Time.fixedDeltaTime = 1f / MarrowGame.xr.Display.GetRecommendedPhysFrequency();

                return false;
            case TimeScaleMode.CLIENT_SIDE_UNSTABLE:
                return true;
            case TimeScaleMode.EVERYONE:
                TimeScaleSender.SendSlowMoButton(true);
                break;
            case TimeScaleMode.HOST_ONLY:
                if (NetworkInfo.IsServer)
                {
                    TimeScaleSender.SendSlowMoButton(true);
                }
                break;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeManager.TOGGLE_TIMESCALE))]
    public static void TOGGLE_TIMESCALE()
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var mode = CommonPreferences.SlowMoMode;

        switch (mode)
        {
            case TimeScaleMode.EVERYONE:
                TimeScaleSender.SendSlowMoButton(false);
                break;
            case TimeScaleMode.HOST_ONLY:
                if (NetworkInfo.IsServer)
                {
                    TimeScaleSender.SendSlowMoButton(false);
                }
                break;
            case TimeScaleMode.LOW_GRAVITY:
                TimeManager.cur_intensity = 1f;

                Time.timeScale = 1f;
                Time.fixedDeltaTime = 1f / MarrowGame.xr.Display.GetRecommendedPhysFrequency();
                break;
        }
    }
}