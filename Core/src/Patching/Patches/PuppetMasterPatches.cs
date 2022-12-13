using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Network;
using LabFusion.Syncables;
using LabFusion.Utilities;
using PuppetMasta;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Muscle))]
    public static class MusclePatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Muscle.FixedUpdate))]
        public static bool FixedUpdate(this Muscle __instance, float t)
        {
            try
            {
                if (NetworkInfo.HasServer && PropSyncable.PuppetMasterCache.TryGetValue(__instance.broadcaster.puppetMaster, out var syncable) && !syncable.IsOwner())
                {
                    __instance.joint.slerpDrive = default;
                    return false;
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("patching Muscle.FixedUpdate", e);
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Muscle.MusclePdDrive))]
        public static bool MusclePdDrive(this Muscle __instance, float muscleWeightMaster, float muscleSpring, float muscleDamper) {
            try {
                if (NetworkInfo.HasServer && PropSyncable.PuppetMasterCache.TryGetValue(__instance.broadcaster.puppetMaster, out var syncable) && !syncable.IsOwner()) {
                    __instance.joint.slerpDrive = default;
                    return false;
                }
            }
            catch (Exception e) {
                FusionLogger.LogException("patching Muscle.MusclePdDrive", e);
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Muscle.UpdateAnchor))]
        public static bool UpdateAnchor(this Muscle __instance) {
            try {
                if (NetworkInfo.HasServer && PropSyncable.PuppetMasterCache.TryGetValue(__instance.broadcaster.puppetMaster, out var syncable) && !syncable.IsOwner()) {
                    __instance.joint.connectedAnchor = __instance._defaultConnectedAnchor;
                    return false;
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("patching Muscle.UpdateAnchor", e);
            }

            return true;
        }
    }
}
