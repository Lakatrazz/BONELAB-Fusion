using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

using SLZ.Rig;
using SLZ.Vehicle;
using SLZ.Marrow.Utilities;
using System.Runtime.InteropServices;

using UnityEngine;
using SLZ.Interaction;
using System.IdentityModel.Tokens;
using LabFusion.Grabbables;
using MelonLoader;
using SLZ.VRMK;
using LabFusion.Senders;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Seat))]
    public static class SeatPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Seat.OnTriggerStay))]
        public static bool OnTriggerStay(Collider other) {
            if (NetworkInfo.HasServer) {
                var grounder = other.GetComponent<PhysGrounder>();

                if (grounder != null && PlayerRepManager.HasPlayerId(grounder.physRig.manager))
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Seat.Register))]
        public static bool Register(Seat __instance, RigManager rM) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                try {
                    if (rM == RigData.RigReferences.RigManager) {
                        MelonCoroutines.Start(Internal_SyncSeat(__instance));
                    }
                    else if (PlayerRepManager.HasPlayerId(rM))
                        return false;
                }
                catch (Exception e)
                {
                    FusionLogger.LogException("patching Seat.Register", e);
                }
            }

            return true;
        }

        private static IEnumerator Internal_SyncSeat(Seat __instance) {
            // Create new syncable if this doesn't exist
            if (!SeatExtender.Cache.ContainsSource(__instance)) {
                bool isAwaiting = true;
                PropSender.SendPropCreation(__instance.gameObject, (p) => {
                    isAwaiting = false;
                });

                while (isAwaiting)
                    yield return null;
            }

            yield return null;

            // Send seat request
            if (__instance.rigManager == RigData.RigReferences.RigManager && SeatExtender.Cache.TryGet(__instance, out var syncable) && syncable.TryGetExtender<SeatExtender>(out var extender))
            {
                using (var writer = FusionWriter.Create(PlayerRepSeatData.Size))
                {
                    using (var data = PlayerRepSeatData.Create(PlayerIdManager.LocalSmallId, syncable.Id, extender.GetIndex(__instance).Value, true))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepSeat, writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Seat.DeRegister))]
        public static void DeRegister(Seat __instance)
        {
            try
            {
                if (NetworkInfo.HasServer && __instance._rig == RigData.RigReferences.RigManager && SeatExtender.Cache.TryGet(__instance, out var syncable) && syncable.TryGetExtender<SeatExtender>(out var extender))
                {
                    using (var writer = FusionWriter.Create(PlayerRepSeatData.Size))
                    {
                        using (var data = PlayerRepSeatData.Create(PlayerIdManager.LocalSmallId, syncable.Id, extender.GetIndex(__instance).Value, false))
                        {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepSeat, writer))
                            {
                                MessageSender.SendToServer(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("patching Seat.Register", e);
            }
        }
    }
}
