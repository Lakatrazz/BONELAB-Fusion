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

using UnityEngine;
using static RootMotion.FinalIK.Grounding;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(GameWorldSkeletonRig))]
    public static class GameworldSkeletonRigPatches {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameWorldSkeletonRig.UpdateHeptaBody2))]
        public static void UpdateHeptaBody2(GameWorldSkeletonRig __instance, Rig inRig, float deltaTime, Vector2 velocity, Vector2 accel) {
            try {
                if (__instance.manager.activeSeat && PlayerRep.Managers.TryGetValue(__instance.manager, out var rep)) {
                    rep.OnHeptaBody2Update();
                }
            }
            catch (Exception e) {
                FusionLogger.LogException("patching GameWorldSkeletonRig.UpdateHeptaBody2", e);
            }
        }
    }
}
