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
            if (!LevelWarehouseUtilities.IsLoadingAllowed && NetworkInfo.HasServer && !NetworkInfo.IsServer) {
                return false;
            }

            return true;
        }

        [HarmonyPatch("Load", typeof(LevelCrateReference), typeof(LevelCrateReference))]
        [HarmonyPrefix]
        public static bool CrateLoad(LevelCrateReference level, LevelCrateReference loadLevel) {
            try {
                // Check if we need to exit early
                if (!LevelWarehouseUtilities.IsLoadingAllowed && NetworkInfo.HasServer && !NetworkInfo.IsServer)
                {
                    return false;
                }

                // A check to make sure we don't try and send an infinite loop of messages. This is also checked server side.
                if (NetworkInfo.IsServer)
                {
                    using (FusionWriter writer = FusionWriter.Create())
                    {
                        using (var data = SceneLoadData.Create(level.Barcode))
                        {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.SceneLoad, writer))
                            {
                                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }

            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch SceneStreamer.Load", e);
#endif
            }

            return true;
        }
    }
}
