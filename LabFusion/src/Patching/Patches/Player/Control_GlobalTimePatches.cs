using HarmonyLib;
using Il2CppSLZ.Bonelab;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(TimeManager))]
    public static class Control_GlobalTimePatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(TimeManager.UNPAUSE))]
        public static void UNPAUSE()
        {
            // Recalibrate player
            // With multiple RigManagers in the scene, the player scale will become 0 when unpausing
            if (RigData.HasPlayer)
            {
                RigData.RigReferences.RigManager.GetComponentInChildren<BodyVitals>().CalibratePlayerBodyScale();
            }
        }

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
}
