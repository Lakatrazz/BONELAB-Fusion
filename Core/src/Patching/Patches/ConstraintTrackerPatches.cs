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
        public static bool DeleteConstraint(ConstraintTracker __instance) {
            if (IgnorePatches || !__instance.isActiveAndEnabled)
                return true;
            
            // Make sure we are in a server and this constraint is actually synced first
            if (NetworkInfo.HasServer && ConstraintSyncable.Cache.TryGet(__instance, out var syncable)) {
                // Make sure the constrainer we are using to delete this is synced
                if (ConstrainerExtender.Cache.TryGet(__instance.source, out var constrainer)) {
                    using var writer = FusionWriter.Create(ConstraintDeleteData.Size);
                    using var data = ConstraintDeleteData.Create(PlayerIdManager.LocalSmallId, constrainer.GetId(), syncable.GetId());
                    writer.Write(data);

                    using var message = FusionMessage.Create(NativeMessageTag.ConstraintDelete, writer);
                    MessageSender.SendToServer(NetworkChannel.Reliable, message);
                }

                // Return false, because constraints are deleted server side
                return false;
            }

            return true;
        }
    }
}
