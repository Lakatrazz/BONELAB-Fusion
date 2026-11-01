using HarmonyLib;

using LabFusion.Network;
using LabFusion.Marrow.Messages;
using LabFusion.Marrow.Rpc;
using LabFusion.Marrow.Extenders;
using LabFusion.Scene;
using LabFusion.Marrow.Rig;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using UnityEngine;

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
            SendSeatCreateAndEnter(__instance);
        }
        else
        {
            return false;
        }

        return true;
    }

    private static void SendSeatCreateAndEnter(Seat seat)
    {
        var marrowBody = MarrowBody.Cache.Get(seat.seatRb.gameObject);

        if (marrowBody == null)
        {
            return;
        }

        if (!SeatExtender.Cache.ContainsSource(seat))
        {
            NetworkMarrowEntityCreator.SendCreateRequest(new NetworkMarrowEntityCreator.MarrowEntityCreateRequestInfo()
            {
                MarrowEntity = marrowBody.Entity,
                CreatedCallback = (info) =>
                {
                    SendSeatEnter(seat);
                },
            });
            return;
        }

        SendSeatEnter(seat);
    }

    private static void SendSeatEnter(Seat seat)
    {
        var rigManager = seat.rigManager;

        if (!NetworkBeingManager.TryGetNetworkRig(rigManager, out var networkRig))
        {
            return;
        }

        var networkRigEntity = networkRig.NetworkEntity;

        if (!networkRigEntity.IsOwner)
        {
            return;
        }

        var seatComponentData = ComponentIndexData.CreateFromComponent<Seat, SeatExtender>(seat, SeatExtender.Cache);

        if (seatComponentData == null)
        {
            return;
        }

        var data = new RigSeatData()
        {
            RigReference = new(networkRigEntity),
            SeatReference = seatComponentData,
            IsSeated = true,
        };

        ClientManager.RelayModule<RigSeatMessage, RigSeatData>(data, CommonMessageRoutes.ReliableToOtherClients);
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

        ClientManager.RelayModule<RigSeatMessage, RigSeatData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }
}