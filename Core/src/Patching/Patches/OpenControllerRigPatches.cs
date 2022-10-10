using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Representation;
using LabFusion.Utilities;
using SLZ.Rig;
using UnityEngine.Rendering;
using UnityEngine;

namespace LabFusion.Patches
{
    [HarmonyPatch(typeof(OpenControllerRig), "OnFixedUpdate")]
    public class OpenFixedUpdatePatch
    {
        public static void Postfix(OpenControllerRig __instance, float deltaTime) {
            if (PlayerRep.Managers.ContainsKey(__instance.manager)) {
                var rep = PlayerRep.Managers[__instance.manager];
                rep.OnUpdateTransforms();
                rep.OnUpdateVelocity();
            }
        }
    }
}
