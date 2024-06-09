using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ConstraintTracker))]
    public static class ConstraintTrackerPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ConstraintTracker.DeleteConstraint))]
        public static bool DeleteConstraint(ConstraintTracker __instance)
        {
            if (IgnorePatches || !__instance.isActiveAndEnabled)
                return true;

            // Make sure we are in a server and this constraint is actually synced first
            if (NetworkInfo.HasServer && ConstraintSyncable.Cache.TryGet(__instance, out var syncable))
            {
                using var writer = FusionWriter.Create(ConstraintDeleteData.Size);
                var data = ConstraintDeleteData.Create(PlayerIdManager.LocalSmallId, syncable.GetId());
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.ConstraintDelete, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);

                // Return false, because constraints are deleted server side
                return false;
            }

            return true;
        }
    }
}
