using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Rig;
using SLZ.VRMK;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(PhysGrounder))]
    public static class PhysGrounderPatches {
        // For some reason, theres a lack of a null check in this method
        // And whatever is null, sometimes makes player reps turn into mush when loading in
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PhysGrounder.UpdateSkid))]
        public static bool UpdateSkid(PhysGrounder __instance, float skidMag) {
            if (NetworkInfo.HasServer && __instance.physRig != RigData.RigReferences.RigManager.physicsRig) {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PhysicsRig))]
    public static class PhysicsRigPatches {
        public static bool ForceAllowUnragdoll = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PhysicsRig.RagdollRig))]
        public static bool RagdollRig(PhysicsRig __instance) {
            try {
                if (NetworkInfo.HasServer && __instance.manager == RigData.RigReferences.RigManager) {
                    var health = __instance.manager.health;

                    // Prevent re-ragdolling when dead/respawning
                    if (!health.alive)
                        return false;

                    using (var writer = FusionWriter.Create(PlayerRepRagdollData.Size)) {
                        using (var data = PlayerRepRagdollData.Create(PlayerIdManager.LocalSmallId, true)) {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepRagdoll, writer)) {
                                MessageSender.SendToServer(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                FusionLogger.LogException("patching PhysicsRig.RagdollRig", e);
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PhysicsRig.UnRagdollRig))]
        public static bool UnRagdollRig(PhysicsRig __instance) {
            try {
                if (NetworkInfo.HasServer && __instance.manager == RigData.RigReferences.RigManager)
                {
                    // Check if we can unragdoll
                    var playerHealth = __instance.manager.health.TryCast<Player_Health>();

                    if (!ForceAllowUnragdoll && playerHealth.deathIsImminent && !FusionPlayer.CanUnragdoll()) {
                        return false;
                    }

                    using (var writer = FusionWriter.Create(PlayerRepRagdollData.Size))
                    {
                        using (var data = PlayerRepRagdollData.Create(PlayerIdManager.LocalSmallId, false))
                        {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepRagdoll, writer))
                            {
                                MessageSender.SendToServer(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                FusionLogger.LogException("patching PhysicsRig.UnRagdollRig", e);
            }

            return true;
        }
    }
}
