using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Entities;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Marrow.Messages;
using LabFusion.Marrow.Extenders;

using UnityEngine;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Marrow.Patching;

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
    public static bool IsReceivingConstraints { get; set; } = false;
    public static ushort FirstId { get; set; }
    public static ushort SecondId { get; set; }

    private static bool IsPlayer(MarrowBody body)
    {
        if (body == null)
        {
            return false;
        }

        if (!MarrowBodyExtender.Cache.TryGet(body, out var entity))
        {
            return false;
        }

        return entity.GetExtender<NetworkPlayer>() != null;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Constrainer.PrimaryButtonUp))]
    public static bool PrimaryButtonUpPrefix(Constrainer __instance)
    {
        if (IsReceivingConstraints)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        // No logic for constraint removing
        if (__instance.mode == Constrainer.ConstraintMode.Remove)
        {
            return true;
        }

        // Prevent player constraining if its disabled
        if (!ConstrainerUtilities.PlayerConstraintsEnabled)
        {
            bool preventPlayer = false;

            if (IsPlayer(__instance._mb1))
            {
                __instance._gO1 = null;
                __instance._mb1 = null;
                preventPlayer = true;
            }

            if (IsPlayer(__instance._mb2))
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

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Constrainer.PrimaryButtonUp))]
    public static void PrimaryButtonUpPostfix(Constrainer __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var go1 = __instance._gO1;
        var go2 = __instance._gO2;

        if (__instance.mode == Constrainer.ConstraintMode.Remove || !go1 || !go2 || go1 == go2)
        {
            return;
        }

        var firstTracker = __instance._gO1.GetComponents<ConstraintTracker>().LastOrDefault();

        if (firstTracker == null)
        {
            return;
        }

        var secondTracker = firstTracker.otherTracker;

        // See if the constrainer is synced
        if (!IsReceivingConstraints && ConstrainerExtender.Cache.TryGet(__instance, out var entity))
        {
            // Delete the constraints and send a creation request
            ConstraintTrackerPatches.IgnorePatches = true;
            firstTracker.DeleteConstraint();
            ConstraintTrackerPatches.IgnorePatches = false;

            // Send create message
            var data = ConstraintCreateData.Create(PlayerIDManager.LocalSmallID, entity.ID, new ConstrainerPointPair(__instance));

            MessageRelay.RelayModule<ConstraintCreateMessage, ConstraintCreateData>(data, CommonMessageRoutes.ReliableToServer);
        }
        // If this is a received message, setup the constraints
        else
        {
            var pointPair = new ConstrainerPointPair(__instance);

            // Register first tracker
            var firstEntity = new NetworkEntity();
            _ = new NetworkConstraint(firstEntity, firstTracker) { PointPair = pointPair, IsFirst = true, OtherId = SecondId, };
            
            NetworkEntityManager.IDManager.RegisterEntity(FirstId, firstEntity);

            // Register second tracker
            var secondEntity = new NetworkEntity();
            _ = new NetworkConstraint(secondEntity, secondTracker) { PointPair = pointPair };

            NetworkEntityManager.IDManager.RegisterEntity(SecondId, secondEntity);

            // Request data catchup
            CatchupManager.RequestEntityDataCatchup(new(firstEntity));
            CatchupManager.RequestEntityDataCatchup(new(secondEntity));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Constrainer.OnTriggerGripUpdate))]
    public static bool OnTriggerGripUpdatePrefix(Constrainer __instance, Hand hand)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (NetworkPlayerManager.HasExternalPlayer(hand.manager))
        {
            return false;
        }

        var constrainerEntity = ConstrainerExtender.Cache.Get(__instance);

        if (constrainerEntity == null)
        {
            return true;
        }

        // Check if the mode was changed
        if (hand.Controller.GetMenuTap())
        {
            // Send mode message
            Constrainer.ConstraintMode nextMode = __instance.mode;

            if (nextMode == Constrainer.ConstraintMode.Remove)
            {
                nextMode = Constrainer.ConstraintMode.Tether;
            }
            else
            {
                nextMode++;
            }

            var data = new ConstrainerModeData()
            {
                ConstrainerID = constrainerEntity.ID,
                Mode = nextMode,
            };

            MessageRelay.RelayModule<ConstrainerModeMessage, ConstrainerModeData>(data, CommonMessageRoutes.ReliableToOtherClients);
        }

        return true;
    }
}
