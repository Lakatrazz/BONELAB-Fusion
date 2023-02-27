using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

using PuppetMasta;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(PuppetMaster))]
    public static class PuppetMasterPatches {
        public static bool IgnorePatches = false;

        [HarmonyPatch(nameof(PuppetMaster.Kill))]
        [HarmonyPrefix]
        [HarmonyPatch(new Type[0])]
        public static bool Kill(PuppetMaster __instance) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && PuppetMasterExtender.Cache.TryGet(__instance, out var syncable)) {
                if (!syncable.IsOwner())
                    return false;
                else {
                    using (var writer = FusionWriter.Create(PuppetMasterKillData.Size))
                    {
                        using (var data = PuppetMasterKillData.Create(PlayerIdManager.LocalSmallId, syncable.Id))
                        {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PuppetMasterKill, writer))
                            {
                                MessageSender.SendToServer(NetworkChannel.Reliable, message);
                            }
                        }
                    }

                    return true;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Muscle))]
    public static class MusclePatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Muscle.MusclePdDrive))]
        public static bool MusclePdDrive(Muscle __instance, float muscleWeightMaster, float muscleSpring, float muscleDamper)
        {
            try
            {
                if (NetworkInfo.HasServer && PuppetMasterExtender.Cache.TryGet(__instance.broadcaster.puppetMaster, out var syncable) && !syncable.IsOwner())
                {
                    __instance.joint.slerpDrive = default;
                    return false;
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("patching Muscle.MusclePdDrive", e);
            }

            return true;
        }
    }
}
