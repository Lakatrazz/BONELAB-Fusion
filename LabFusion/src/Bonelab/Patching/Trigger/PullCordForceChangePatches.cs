using HarmonyLib;

using LabFusion.Network;
using LabFusion.Entities;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Interaction;

using UnityEngine;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(PullCordForceChange))]
public static class PullCordForceChangePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordForceChange.OnTriggerEnter))]
    public static bool OnTriggerEnter(Collider other)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        var rigidbody = other.attachedRigidbody;

        if (rigidbody == null)
        {
            return true;
        }

        var marrowBody = MarrowBody.Cache.Get(rigidbody.gameObject);

        if (marrowBody == null)
        {
            return true;
        }

        var marrowEntity = marrowBody.Entity;

        // Check if the triggered marrow entity has a network entity attached
        var networkEntity = IMarrowEntityExtender.Cache.Get(marrowEntity);

        if (networkEntity == null)
        {
            return true;
        }

        // Only let pull cord force changes trigger for entities that are owned by us (ex. our own player)
        return networkEntity.IsOwner;
    }
}