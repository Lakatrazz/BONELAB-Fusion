using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Rig;

using UnityEngine;

namespace LabFusion.Patching;

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
        var rigManager = RigManager.Cache.Get(marrowEntity.gameObject);

        if (rigManager != null)
        {
            return rigManager.IsSelf();
        }

        return true;
    }
}