using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Rig;

using UnityEngine;

using Avatar = SLZ.VRMK.Avatar;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(RigManager))]
    public static class RigManagerPatches {
        [HarmonyPatch(nameof(RigManager.SwitchAvatar))]
        [HarmonyPostfix]
        public static void SwitchAvatar(RigManager __instance, Avatar newAvatar) {
            try {
                MelonCoroutines.Start(Internal_WaitForBarcode(__instance, newAvatar));
            }
            catch (Exception e) {
                FusionLogger.LogException("patching RigManager.SwitchAvatar", e);
            }
        }

        private static IEnumerator Internal_WaitForBarcode(RigManager __instance, Avatar newAvatar) {
            yield return null;

            if (__instance.IsNOC())
                yield break;

            // Is this our local player? If so, sync the avatar change
            if (__instance.IsLocalPlayer()) {
                FusionPlayer.Internal_OnAvatarChanged(__instance, newAvatar, __instance.AvatarCrate.Barcode);
            }
        }
    }
}
