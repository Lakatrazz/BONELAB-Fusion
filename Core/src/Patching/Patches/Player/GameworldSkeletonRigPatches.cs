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
    [HarmonyPatch(typeof(GameWorldSkeletonRig))]
    public static class GameworldSkeletonRigPatches {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameWorldSkeletonRig.OnFixedUpdate))]
        public static void OnFixedUpdate(GameWorldSkeletonRig __instance, float deltaTime) {
            try {
                if (__instance.manager.activeSeat && PlayerRepManager.TryGetPlayerRep(__instance.manager, out var rep)) {
                    rep.OnHeptaBody2Update();
                }
            }
            catch (Exception e) {
                FusionLogger.LogException("patching GameWorldSkeletonRig.OnFixedUpdate", e);
            }
        }
    }
}
