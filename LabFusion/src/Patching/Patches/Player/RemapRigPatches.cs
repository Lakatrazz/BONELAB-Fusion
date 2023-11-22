using HarmonyLib;
using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Utilities;
using SLZ.Rig;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(RemapRig))]
    public static class RemapRigPatches
    {
        private static bool _wasChargingInput;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(RemapRig.JumpCharge))]
        public static void JumpCharge(RemapRig __instance, float deltaTime, bool chargeInput, bool __state)
        {
            if (NetworkInfo.HasServer && __instance.manager.IsSelf())
            {
                if (_wasChargingInput && !chargeInput)
                {
                    PlayerSender.SendPlayerAction(PlayerActionType.JUMP);
                }

                _wasChargingInput = chargeInput;
            }
        }
    }
}
