using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

using MelonLoader;

using SLZ.Bonelab;
using SLZ.Interaction;
using SLZ.Props;

using UnityEngine;

namespace LabFusion.Patching {
    public struct BoardPointPair {
        public Vector3 point1;
        public Vector3 point2;
    }

    [HarmonyPatch(typeof(BoardGenerator))]
    public static class BoardGeneratorPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BoardGenerator.BoardSpawner))]
        public static bool BoardSpawner(BoardGenerator __instance, int idx, float mass)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                // See if the board generator is synced
                if (BoardGeneratorExtender.Cache.TryGet(__instance, out var syncable)) {
                    if (syncable.IsOwner()) {
                        // Send board create message
                        using (var writer = FusionWriter.Create(BoardCreateData.Size))
                        {
                            using (var data = BoardCreateData.Create(PlayerIdManager.LocalSmallId, syncable.GetId(), __instance, idx, mass))
                            {
                                writer.Write(data);

                                using (var message = FusionMessage.Create(NativeMessageTag.BoardCreate, writer))
                                {
                                    MessageSender.SendToServer(NetworkChannel.Reliable, message);
                                }
                            }
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
