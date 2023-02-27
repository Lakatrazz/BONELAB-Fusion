using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;

using SLZ.Props;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ConstraintTracker))]
    public static class ConstraintTrackerPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ConstraintTracker.DeleteConstraint))]
        public static void DeleteConstraint(ConstraintTracker __instance) {
            if (IgnorePatches)
                return;
            
            if (NetworkInfo.HasServer && ConstraintSyncable.Cache.TryGet(__instance, out var syncable)) {
                using (var writer = FusionWriter.Create(ConstraintDeleteData.Size)) {
                    using (var data = ConstraintDeleteData.Create(PlayerIdManager.LocalSmallId, syncable.GetId()))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.ConstraintDelete, writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
