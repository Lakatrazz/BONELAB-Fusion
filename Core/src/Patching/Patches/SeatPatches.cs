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

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Seat))]
    public static class SeatPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Seat.Register))]
        public static void Register(Seat __instance, RigManager rM) {
            try {
                if (NetworkInfo.HasServer && rM == RigData.RigReferences.RigManager) {
                    MelonCoroutines.Start(Internal_SyncSeat(__instance));
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("patching Seat.Register", e);
            }
        }

        private static IEnumerator Internal_SyncSeat(Seat __instance) {
            // Create new syncable if this doesn't exist
            if (!SeatExtender.Cache.ContainsSource(__instance)) {
                // We aren't a server. Request an id.
                if (!NetworkInfo.IsServer) {
                    // Get grip host
                    var host = __instance.GetComponentInParent<InteractableHost>();
                    if (!host)
                        host = __instance.GetComponentInChildren<InteractableHost>();

                    var newSyncable = new PropSyncable(host);

                    ushort queuedId = SyncManager.QueueSyncable(newSyncable);

                    using (var writer = FusionWriter.Create())
                    {
                        using (var data = SyncableIDRequestData.Create(PlayerIdManager.LocalSmallId, queuedId))
                        {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.SyncableIDRequest, writer))
                            {
                                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                            }
                        }
                    }

                    while (newSyncable.IsQueued())
                        yield return null;

                    yield return null;

                    using (var writer = FusionWriter.Create()) {
                        using (var data = PropSyncableCreateData.Create(PlayerIdManager.LocalSmallId, host.gameObject.GetFullPath(), queuedId)) {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PropSyncableCreate, writer)) {
                                MessageSender.SendToServer(NetworkChannel.Reliable, message);
                            }
                        }
                    }

                    yield return null;
                }
                else if (NetworkInfo.IsServer)
                {
                    // Get grip host
                    var host = __instance.GetComponentInParent<InteractableHost>();
                    if (!host)
                        host = __instance.GetComponentInChildren<InteractableHost>();

                    var newSyncable = new PropSyncable(host);
                    SyncManager.RegisterSyncable(newSyncable, SyncManager.AllocateSyncID());

                    using (var writer = FusionWriter.Create()) {
                        using (var data = PropSyncableCreateData.Create(PlayerIdManager.LocalSmallId, host.gameObject.GetFullPath(), newSyncable.Id)) {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PropSyncableCreate, writer)) {
                                MessageSender.SendToServer(NetworkChannel.Reliable, message);
                            }
                        }
                    }

                    yield return null;
                }
            }

            yield return null;

            // Send seat request
            if (__instance.rigManager == RigData.RigReferences.RigManager && SeatExtender.Cache.TryGet(__instance, out var syncable) && syncable.TryGetExtender<SeatExtender>(out var extender))
            {
                using (var writer = FusionWriter.Create())
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
                    using (var writer = FusionWriter.Create())
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
