using HarmonyLib;
using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Syncables;
using SLZ.Bonelab;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Powerable_Joint))]
    public static class PowerableJointPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Powerable_Joint.SETJOINT))]
        public static bool SETJOINT(Powerable_Joint __instance, float voltage)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && PowerableJointExtender.Cache.TryGet(__instance, out var syncable))
            {
                if (syncable.IsOwner())
                {
                    PowerableSender.SendPowerableJointVoltage(syncable.GetId(), voltage);
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}
