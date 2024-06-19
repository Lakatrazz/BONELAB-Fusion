using HarmonyLib;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Entities;
using LabFusion.Utilities;
using Il2CppSLZ.Interaction;
using UnityEngine;
using Il2CppSLZ.Bonelab;

namespace LabFusion.Patching
{
    public struct ConstrainerPointPair
    {
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

            go1 = constrainer._mb1 ? constrainer._mb1.gameObject : constrainer._gO1;
            go2 = constrainer._mb2 ? constrainer._mb2.gameObject : constrainer._gO2;
        }
    }

    [HarmonyPatch(typeof(Constrainer))]
    public static class ConstrainerPatches
    {

        public static bool IsReceivingConstraints = false;
        public static ushort FirstId;
        public static ushort SecondId;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Constrainer.PrimaryButtonUp))]
        public static bool PrimaryButtonUpPrefix(Constrainer __instance)
        {
            if (IsReceivingConstraints)
                return true;

            if (NetworkInfo.HasServer)
            {
                // No logic for constraint removing
                if (__instance.mode == Constrainer.ConstraintMode.Remove)
                {
                    return true;
                }

                // Prevent player constraining if its disabled
                if (!ConstrainerUtilities.PlayerConstraintsEnabled)
                {
                    bool preventPlayer = false;

                    if (__instance._mb1.IsPartOfPlayer())
                    {
                        __instance._gO1 = null;
                        __instance._mb1 = null;
                        preventPlayer = true;
                    }

                    if (__instance._mb2.IsPartOfPlayer())
                    {
                        __instance._gO2 = null;
                        __instance._mb2 = null;
                        preventPlayer = true;
                    }

                    if (preventPlayer)
                    {
                        __instance._raycastMissedOnDown = true;
                        __instance.sfx.Release();
                        return false;
                    }
                }
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Constrainer.PrimaryButtonUp))]
        public static void PrimaryButtonUpPostfix(Constrainer __instance)
        {
            var go1 = __instance._gO1;
            var go2 = __instance._gO2;

            if (__instance.mode == Constrainer.ConstraintMode.Remove || !go1 || !go2 || go1 == go2)
            {
                return;
            }

            if (!NetworkInfo.HasServer)
            {
                return;
            }

            var firstTracker = __instance._gO1.GetComponents<ConstraintTracker>().LastOrDefault();

            if (firstTracker == null)
                return;

            var secondTracker = firstTracker.otherTracker;

            // See if the constrainer is synced
            if (!IsReceivingConstraints && ConstrainerExtender.Cache.TryGet(__instance, out var entity))
            {
                // Delete the constraints and send a creation request
                ConstraintTrackerPatches.IgnorePatches = true;
                firstTracker.DeleteConstraint();
                ConstraintTrackerPatches.IgnorePatches = false;

                // Send create message
                using var writer = FusionWriter.Create(ConstraintCreateData.Size);
                var data = ConstraintCreateData.Create(PlayerIdManager.LocalSmallId, entity.Id, new ConstrainerPointPair(__instance));
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.ConstraintCreate, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);
            }
            // If this is a received message, setup the constraints
            else
            {
                // Register first tracker
                var firstEntity = new NetworkEntity();
                var firstConstraint = new NetworkConstraint(firstEntity, firstTracker);

                NetworkEntityManager.IdManager.RegisterEntity(FirstId, firstEntity);

                // Register second tracker
                var secondEntity = new NetworkEntity();
                var secondConstraint = new NetworkConstraint(secondEntity, secondTracker);

                NetworkEntityManager.IdManager.RegisterEntity(SecondId, secondEntity);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Constrainer.OnTriggerGripUpdate))]
        public static bool OnTriggerGripUpdatePrefix(Constrainer __instance, Hand hand)
        {
            if (NetworkInfo.HasServer)
            {
                if (NetworkPlayerManager.HasExternalPlayer(hand.manager))
                {
                    return false;
                }
                else if (ConstrainerExtender.Cache.TryGet(__instance, out var syncable))
                {
                    // Check if the mode was changed
                    if (hand.Controller.GetMenuTap())
                    {
                        // Send mode message
                        Constrainer.ConstraintMode nextMode = __instance.mode;
                        if (nextMode == Constrainer.ConstraintMode.Remove)
                            nextMode = Constrainer.ConstraintMode.Tether;
                        else
                            nextMode++;

                        using var writer = FusionWriter.Create(ConstrainerModeData.Size);
                        var data = ConstrainerModeData.Create(PlayerIdManager.LocalSmallId, syncable.Id, nextMode);
                        writer.Write(data);

                        using var message = FusionMessage.Create(NativeMessageTag.ConstrainerMode, writer);
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }

            return true;
        }
    }
}
