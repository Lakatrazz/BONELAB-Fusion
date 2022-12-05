using System;
using System.Collections.Generic;
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

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Seat))]
    public static class SeatPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Seat.Register))]
        public static void Register(Seat __instance, RigManager rM) {
            try {
                if (NetworkInfo.HasServer && rM == RigData.RigReferences.RigManager && PropSyncable.SeatCache.TryGetValue(__instance, out var syncable)) {
                    using (var writer = FusionWriter.Create()) {
                        using (var data = PlayerRepSeatData.Create(PlayerIdManager.LocalSmallId, syncable.Id, syncable.GetIndex(__instance).Value, true)) {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepSeat, writer)) {
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

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Seat.DeRegister))]
        public static void DeRegister(Seat __instance)
        {
            try
            {
                if (NetworkInfo.HasServer && __instance._rig == RigData.RigReferences.RigManager && PropSyncable.SeatCache.TryGetValue(__instance, out var syncable))
                {
                    using (var writer = FusionWriter.Create())
                    {
                        using (var data = PlayerRepSeatData.Create(PlayerIdManager.LocalSmallId, syncable.Id, syncable.GetIndex(__instance).Value, false))
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
