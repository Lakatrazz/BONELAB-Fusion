using HarmonyLib;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Utilities;

using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;

using UnityEngine;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(ObjectCleanupVolume))]
public static class ObjectCleanupVolumePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ObjectCleanupVolume.OnTriggerEnter))]
    public static void OnTriggerEnter(Collider col)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var attachedRigidbody = col.attachedRigidbody;

        if (attachedRigidbody == null)
        {
            return;
        }

        var marrowBody = MarrowBody.Cache.Get(attachedRigidbody.gameObject);

        if (marrowBody == null)
        {
            return;
        }

        var marrowEntity = marrowBody.Entity;

        if (!IMarrowEntityExtender.Cache.TryGet(marrowEntity, out var networkEntity))
        {
            return;
        }

        // Check if this NetworkEntity is owned by us
        if (!networkEntity.IsOwner)
        {
            return;
        }

        // If this is a player, and it's owned by us, then it is us
        var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

        if (networkPlayer == null)
        {
            return;
        }

        // Instead of killing the player (doesn't work with immortality), teleport them to spawn
        LocalPlayer.TeleportToCheckpoint();
    }
}
