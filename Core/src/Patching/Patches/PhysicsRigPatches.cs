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

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(PhysicsRig))]
    public static class PhysicsRigPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PhysicsRig.RagdollRig))]
        public static void RagdollRig(PhysicsRig __instance) {
            try {
                if (NetworkInfo.HasServer && __instance.manager == RigData.RigReferences.RigManager) {
                    using (var writer = FusionWriter.Create()) {
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
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PhysicsRig.UnRagdollRig))]
        public static void UnRagdollRig(PhysicsRig __instance) {
            try {
                if (NetworkInfo.HasServer && __instance.manager == RigData.RigReferences.RigManager)
                {
                    using (var writer = FusionWriter.Create())
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
        }
    }
}
