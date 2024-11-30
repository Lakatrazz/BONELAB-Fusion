using HarmonyLib;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.AI;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Utilities;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

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

        // Only check for TriggerRefProxy so we don't accidentally trigger on something like the Body Log
        var triggerRefProxy = marrowBody.GetComponent<TriggerRefProxy>();

        if (triggerRefProxy == null || triggerRefProxy.root == null)
        {
            return;
        }

        var rigManager = RigManager.Cache.Get(triggerRefProxy.root);

        if (rigManager == null || !rigManager.IsLocalPlayer())
        {
            return;
        }

        // Instead of killing the player (doesn't work with immortality), teleport them to spawn
        LocalPlayer.TeleportToCheckpoint();
    }
}
