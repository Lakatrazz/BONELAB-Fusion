using HarmonyLib;

using System;

using LabFusion.Network;
using LabFusion.Extensions;

using SLZ.Zones;

using UnityEngine;

using System.Collections.Generic;

using LabFusion.Utilities;

using SLZ.AI;
using SLZ.Rig;

using LabFusion.Data;

using SLZ.Marrow.SceneStreaming;

namespace Entanglement.Patching
{
    [HarmonyPatch(typeof(ChunkTrigger), "OnTriggerEnter")]
    public static class ChunkEnterPatch
    {
        public static bool Prefix(ChunkTrigger __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var trigger = TriggerRefProxy.Cache.Get(other.gameObject);
                RigManager rig;

                if (trigger && trigger.root && (rig = RigManager.Cache.Get(trigger.root))) {
                    return rig == RigData.RigManager;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ChunkTrigger), "OnTriggerExit")]
    public static class ChunkExitPatch
    {
        public static bool Prefix(ChunkTrigger __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var trigger = TriggerRefProxy.Cache.Get(other.gameObject);
                RigManager rig;

                if (trigger && trigger.root && (rig = RigManager.Cache.Get(trigger.root))) {
                    return rig == RigData.RigManager;
                }
            }

            return true;
        }
    }
}

