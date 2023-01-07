using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Rig;
using SLZ.VRMK;

using UnityEngine;

using Avatar = SLZ.VRMK.Avatar;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(Avatar))]
    public static class AvatarPatches {
        [HarmonyPatch(nameof(Avatar.RefreshBodyMeasurements))]
        [HarmonyPatch(new Type[0])]
        [HarmonyPostfix]
        public static void RefreshBodyMeasurements(Avatar __instance) {
            try {
                if (NetworkInfo.HasServer) {
                    var rm = __instance.GetComponentInParent<RigManager>();

                    if (rm != null && PlayerRep.Managers.TryGetValue(rm, out var rep) && rep.avatarStats != null) {
                        // Apply the avatar stats
                        rep.avatarStats.CopyTo(__instance);

                        // Scale the mesh if its poly blank
                        var go = __instance.gameObject;
                        if (go.name.Contains("char_marrow1_polyBlank")) {
                            go.transform.localScale = Vector3.one * (__instance._height / 1.76f);
                        }
                    }
                }
            }
            catch (Exception e) {
                FusionLogger.LogException("patching Avatar.RefreshBodyMeasurements", e);
            }
        }
    }
}
