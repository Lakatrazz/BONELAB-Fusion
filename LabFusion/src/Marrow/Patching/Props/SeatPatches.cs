using System.Collections;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Marrow.Messages;
using LabFusion.Entities;
using LabFusion.Senders;
using LabFusion.Marrow.Extenders;
using LabFusion.Scene;
using LabFusion.Marrow.Rig;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using UnityEngine;

using MelonLoader;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(Seat))]
public static class SeatPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Seat.OnTriggerStay))]
    public static bool OnTriggerStay(Collider other)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        var grounder = other.GetComponent<PhysGrounder>();

        if (grounder != null && !NetworkBeingManager.HasOwnership(grounder.physRig.manager))
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Seat.Register))]
    public static bool Register(Seat __instance, RigManager rM)
    {
        if (IgnorePatches)
        {
            IgnorePatches = false;
            return true;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (rM.HasOwnership())
        {
            MelonCoroutines.Start(SendSeatEnter(__instance));
        }
        else
        {
            return false;
        }

        return true;
    }

    private static IEnumerator SendSeatEnter(Seat __instance)
    {
        var marrowBody = MarrowBody.Cache.Get(__instance.seatRb.gameObject);

        if (marrowBody == null)
        {
            yield break;
        }

        // Create new syncable if this doesn't exist
        if (!SeatExtender.Cache.ContainsSource(__instance))
        {
            bool isAwaiting = true;
            PropSender.SendPropCreation(marrowBody.Entity, (p) =>
            {
                isAwaiting = false;
            });

            while (isAwaiting)
            {
                yield return null;
            }
        }

        yield return null;

        // Send seat request
        var rigManager = __instance.rigManager;

        if (!NetworkBeingManager.TryGetNetworkRig(rigManager, out var networkRig))
        {
            yield break;
        }

        var networkRigEntity = networkRig.NetworkEntity;

        if (!networkRigEntity.IsOwner)
        {
            yield break;
        }

        var seatComponentData = ComponentIndexData.CreateFromComponent<Seat, SeatExtender>(__instance, SeatExtender.Cache);

        if (seatComponentData == null)
        {
            yield break;
        }

        var data = new RigSeatData()
        {
            RigReference = new(networkRigEntity),
            SeatReference = seatComponentData,
            IsSeated = true,
        };

        MessageRelay.RelayModule<RigSeatMessage, RigSeatData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Seat.DeRegister))]
    public static void DeRegister(Seat __instance)
    {
        if (IgnorePatches)
        {
            IgnorePatches = false;
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var rigManager = __instance.rigManager;

        if (!NetworkBeingManager.TryGetNetworkRig(rigManager, out var networkRig))
        {
            return;
        }

        var networkRigEntity = networkRig.NetworkEntity;

        if (!networkRigEntity.IsOwner)
        {
            return;
        }

        var seatComponentData = ComponentIndexData.CreateFromComponent<Seat, SeatExtender>(__instance, SeatExtender.Cache);

        if (seatComponentData == null)
        {
            return;
        }

        var data = new RigSeatData()
        {
            RigReference = new(networkRigEntity),
            SeatReference = seatComponentData,
            IsSeated = false,
        };

        MessageRelay.RelayModule<RigSeatMessage, RigSeatData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }
}