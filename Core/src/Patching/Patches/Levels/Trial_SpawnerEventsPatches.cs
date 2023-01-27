using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;

using SLZ.Bonelab;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(Trial_SpawnerEvents))]
    public static class Trial_SpawnerEventsPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Trial_SpawnerEvents.OnSpawnerDeath))]
        public static bool OnSpawnerDeath(Trial_SpawnerEvents __instance) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (NetworkInfo.IsServer) {
                    TrialSender.SendTrialSpawnerEvent(__instance);
                }
                else
                    return false;
            }

            return true;
        }
    }
}
