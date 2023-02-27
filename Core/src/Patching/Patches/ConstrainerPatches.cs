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
using SLZ.Interaction;
using SLZ.Props;
using UnityEngine;

namespace LabFusion.Patching {
    public struct ConstrainerPointPair {
        public Constrainer.ConstraintMode mode;

        public Vector3 point1;
        public Vector3 point2;

        public Vector3 normal1;
        public Vector3 normal2;

        public GameObject go1;
        public GameObject go2;

        public ConstrainerPointPair(Constrainer constrainer)
        {
            mode = constrainer.mode;

            point1 = constrainer._point1;
            point2 = constrainer._point2;

            normal1 = constrainer._normal1;
            normal2 = constrainer._normal2;

            go1 = constrainer._rb1 ? constrainer._rb1.gameObject : constrainer._gO1;
            go2 = constrainer._rb2 ? constrainer._rb2.gameObject : constrainer._gO2;
        }
    }

    [HarmonyPatch(typeof(Constrainer))]
    public static class ConstrainerPatches {

        public static bool IsReceivingConstraints = false;
        public static ushort FirstId;
        public static ushort SecondId;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Constrainer.PrimaryButtonUp))]
        public static void PrimaryButtonUp(Constrainer __instance)
        {
            var go1 = __instance._gO1;
            var go2 = __instance._gO2;
            if (__instance.mode == Constrainer.ConstraintMode.Remove || !go1 || !go2 || go1 == go2)
                return;

            if (NetworkInfo.HasServer)
            {
                var firstTracker = __instance._gO1.GetComponents<ConstraintTracker>().LastOrDefault();

                if (firstTracker == null)
                    return;

                var secondTracker = firstTracker.otherTracker;

                // See if the constrainer is synced
                if (!IsReceivingConstraints && ConstrainerExtender.Cache.TryGet(__instance, out var syncable))
                {
                    // Now, sync the start and end position, and constraint mode
                    var firstSyncable = new ConstraintSyncable(firstTracker);
                    var secondSyncable = new ConstraintSyncable(secondTracker);
                    
                    MelonCoroutines.Start(CoDelaySyncConstraints(new ConstrainerPointPair(__instance), syncable.GetId(), firstSyncable, secondSyncable));
                }
                // If this is a received message, setup the constraints
                else {
                    var firstSyncable = new ConstraintSyncable(firstTracker);
                    SyncManager.RegisterSyncable(firstSyncable, FirstId);

                    var secondSyncable = new ConstraintSyncable(secondTracker);
                    SyncManager.RegisterSyncable(secondSyncable, SecondId);
                }
            }
        }

        private static IEnumerator CoDelaySyncConstraints(ConstrainerPointPair pair, ushort constrainerId, ConstraintSyncable first, ConstraintSyncable other)
        {
            // Get id for first tracker
            ushort queuedId = SyncManager.QueueSyncable(first);
            
            SyncManager.RequestSyncableID(queuedId);

            while (first.IsQueued())
                yield return null;

            // Get id for second tracker
            queuedId = SyncManager.QueueSyncable(other);

            SyncManager.RequestSyncableID(queuedId);

            while (other.IsQueued())
                yield return null;

            // Send create message
            using (var writer = FusionWriter.Create(ConstraintCreateData.Size))
            {
                using (var data = ConstraintCreateData.Create(PlayerIdManager.LocalSmallId, constrainerId, pair, first, other))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.ConstraintCreate, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Constrainer.OnTriggerGripUpdate))]
        public static bool OnTriggerGripUpdate(Hand hand) {
            if (NetworkInfo.HasServer && PlayerRepManager.HasPlayerId(hand.manager))
                return false;

            return true;
        }
    }
}
