using HarmonyLib;

using System;
using System.Collections.Generic;

using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Extensions;

using SLZ.AI;
using SLZ.Rig;
using SLZ.Zones;
using SLZ.Bonelab;

using LabFusion.SDK.Points;
using LabFusion.Representation;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Mirror))]
    public static class MirrorPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Mirror.OnTriggerEnter))]
        public static bool OnTriggerEnter(Mirror __instance, Collider c) {
            if (c.CompareTag("Player")) {
                bool isMainRig = TriggerUtilities.IsMainRig(c);

                if (isMainRig) {
                    var rigManager = RigManager.Cache.Get(TriggerRefProxy.Cache.Get(c.gameObject).root);

                    foreach (var item in PointItemManager.LoadedItems) {
                        if (item.IsEquipped) {
                            item.OnUpdateObjects(new PointItemPayload()
                            {
                                type = PointItemPayloadType.MIRROR,
                                rigManager = rigManager,
                                mirror = __instance,
                                playerId = PlayerIdManager.LocalId,
                            }, true);
                        }
                    }
                }

                return isMainRig;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Mirror.OnTriggerExit))]
        public static bool OnTriggerExit(Mirror __instance, Collider c) {
            if (c.CompareTag("Player")) {
                bool isMainRig = TriggerUtilities.IsMainRig(c);

                if (isMainRig) {
                    var rigManager = RigManager.Cache.Get(TriggerRefProxy.Cache.Get(c.gameObject).root);

                    foreach (var item in PointItemManager.LoadedItems) {
                        if (item.IsEquipped) {
                            item.OnUpdateObjects(new PointItemPayload()
                            {
                                type = PointItemPayloadType.MIRROR,
                                rigManager = rigManager,
                                mirror = __instance,
                                playerId = PlayerIdManager.LocalId,
                            }, false);
                        }
                    }
                }

                return isMainRig;
            }


            return true;
        }
    }

    [HarmonyPatch(typeof(TriggerLasers), "OnTriggerEnter")]
    public static class PlayerTriggerEnterPatch
    {
        public static bool Prefix(TriggerLasers __instance, Collider other)
        {
            if (other.CompareTag("Player")) {
                if (TriggerUtilities.VerifyLevelTrigger(__instance, other, out bool runMethod))
                    return runMethod;

                TriggerUtilities.Increment(__instance);
                bool canEnter = TriggerUtilities.CanEnter(__instance);

                return canEnter;
            }

            return true;
        }

        public static void Postfix(TriggerLasers __instance, Collider other) {
            if (__instance.rigManager != null && __instance.rigManager != RigData.RigReferences.RigManager)
                __instance.rigManager = RigData.RigReferences.RigManager;
        }
    }

    [HarmonyPatch(typeof(TriggerLasers), "OnTriggerExit")]
    public static class PlayerTriggerExitPatch
    {
        public static bool Prefix(TriggerLasers __instance, Collider other)
        {
            if (other.CompareTag("Player")) {
                if (TriggerUtilities.VerifyLevelTrigger(__instance, other, out bool runMethod))
                    return runMethod;

                TriggerUtilities.Decrement(__instance);
                bool canExit = TriggerUtilities.CanExit(__instance);

                return canExit;
            }

            return true;
        }

        public static void Postfix(TriggerLasers __instance, Collider other) {
            if (__instance.rigManager != null && __instance.rigManager != RigData.RigReferences.RigManager)
                __instance.rigManager = RigData.RigReferences.RigManager;
        }
    }
}

