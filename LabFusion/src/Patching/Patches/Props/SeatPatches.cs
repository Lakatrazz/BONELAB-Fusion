using System.Collections;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Entities;
using LabFusion.Utilities;
using LabFusion.Senders;
using LabFusion.Marrow.Extenders;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using UnityEngine;

using MelonLoader;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(Seat))]
public static class SeatPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Seat.OnTriggerStay))]
    public static bool OnTriggerStay(Collider other)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        var grounder = other.GetComponent<PhysGrounder>();

        if (grounder != null && NetworkPlayerManager.HasExternalPlayer(grounder.physRig.manager))
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

        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        if (rM.IsLocalPlayer())
        {
            MelonCoroutines.Start(Internal_SyncSeat(__instance));
        }
        else if (NetworkPlayerManager.HasExternalPlayer(rM))
        {
            return false;
        }

        return true;
    }

    private static IEnumerator Internal_SyncSeat(Seat __instance)
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
                yield return null;
        }

        yield return null;

        // Send seat request
        if (!__instance.rigManager.IsLocalPlayer())
        {
            yield break;
        }

        var entity = SeatExtender.Cache.Get(__instance);

        if (entity == null)
        {
            yield break;
        }

        var extender = entity.GetExtender<SeatExtender>();

        var data = PlayerRepSeatData.Create(PlayerIDManager.LocalSmallID, entity.ID, (byte)extender.GetIndex(__instance).Value, true);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepSeat, CommonMessageRoutes.ReliableToOtherClients);
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

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!__instance._rig.IsLocalPlayer())
        {
            return;
        }

        var entity = SeatExtender.Cache.Get(__instance);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<SeatExtender>();

        var data = PlayerRepSeatData.Create(PlayerIDManager.LocalSmallID, entity.ID, (byte)extender.GetIndex(__instance).Value, false);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepSeat, CommonMessageRoutes.ReliableToOtherClients);
    }
}