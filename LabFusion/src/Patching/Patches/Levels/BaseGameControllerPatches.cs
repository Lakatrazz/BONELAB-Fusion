﻿using HarmonyLib;
using LabFusion.Network;
using LabFusion.Senders;
using SLZ.Bonelab;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(BaseGameController))]
    public static class BaseGameControllerPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BaseGameController.BeginSession))]
        public static bool BeginSession()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                GameControllerSender.SendGameControllerEvent(BaseGameControllerType.BeginSession);
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BaseGameController.EndSession))]
        public static bool EndSession()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                GameControllerSender.SendGameControllerEvent(BaseGameControllerType.EndSession);
            }

            return true;
        }
    }
}
