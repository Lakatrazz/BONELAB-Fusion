using HarmonyLib;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.VoidLogic;

using LabFusion.Entities;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(DamageVolume))]
public static class DamageVolumePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DamageVolume.OnTriggerEnter))]
    public static bool OnTriggerEnterPrefix(Collider other)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        var attachedRigidbody = other.attachedRigidbody;

        if (attachedRigidbody == null)
        {
            return true;
        }

        var marrowBody = MarrowBody.Cache.Get(attachedRigidbody.gameObject);

        if (marrowBody == null)
        {
            return true;
        }

        var marrowEntity = marrowBody.Entity;

        if (!IMarrowEntityExtender.Cache.TryGet(marrowEntity, out var networkEntity))
        {
            return true;
        }

        // Don't include networked players in the damage volume, as damage volumes break with more than one RigManager
        if (networkEntity.GetExtender<NetworkPlayer>() != null && !networkEntity.IsOwner)
        {
            return false;
        }

        return true;
    }
}
