using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using SLZ.Rig;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(RemapRig))]
    public static class RemapRigPatches {
        private static bool _wasChargingInput = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(RemapRig.JumpCharge))]
        public static void JumpCharge(RemapRig __instance, float deltaTime, bool chargeInput, bool __state)
        {
            if (NetworkInfo.HasServer && __instance.manager.IsLocalPlayer()) {
                if (_wasChargingInput && !chargeInput) {
                    PlayerSender.SendPlayerAction(PlayerActionType.JUMP);
                }

                _wasChargingInput = chargeInput;
            }
        }
    }
}
