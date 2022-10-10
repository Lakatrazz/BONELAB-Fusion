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
            foreach (var pair in PlayerRep.Managers) {
                if (pair.Key == __instance.manager) {
                    pair.Value.OnUpdateTransforms();
                    pair.Value.OnUpdateVelocity();
                }
            }
        }
    }
}
