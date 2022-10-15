using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;

using SLZ.Bonelab;
using SLZ.Marrow.SceneStreaming;
using SLZ.Marrow.Warehouse;
using SLZ.SceneStreaming;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(DoorPortalController), "OnTriggerEnter")]
    public class DoorPortalPatch {
        public static bool Prefix(Collider other) {
            if (other.CompareTag("Player")) {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SceneStreamer))]
    public class SceneLoadPatch {
        [HarmonyPatch("Load", typeof(string), typeof(string))]
        [HarmonyPrefix]
        public static bool StringLoad(string levelBarcode, string loadLevelBarcode = "") {
            // Check if we need to exit early
            if (!LevelWarehouseUtilities.IsLoadingAllowed && NetworkUtilities.HasServer && !NetworkUtilities.IsServer) {
                return false;
            }

            return true;
        }

        [HarmonyPatch("Load", typeof(LevelCrateReference), typeof(LevelCrateReference))]
        [HarmonyPrefix]
        public static bool CrateLoad(LevelCrateReference level, LevelCrateReference loadLevel) {
            // Check if we need to exit early
            if (!LevelWarehouseUtilities.IsLoadingAllowed && NetworkUtilities.HasServer && !NetworkUtilities.IsServer) {
                return false;
            }

            // A check to make sure we don't try and send an infinite loop of messages. This is also checked server side.
            if (NetworkUtilities.IsServer) {
                using (FusionWriter writer = FusionWriter.Create()) {
                    using (var data = SceneLoadData.Create(level.Barcode)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.SceneLoad, writer)) {
                            NetworkUtilities.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }

            return true;
        }
    }
}
