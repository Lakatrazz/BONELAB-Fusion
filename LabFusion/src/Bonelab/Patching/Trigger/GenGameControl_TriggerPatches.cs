using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.AI;
using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GenGameControl_Trigger))]
public static class GenGameControl_TriggerPatches
{
    private static bool ProcessTrigger(GenGameControl_Trigger __instance, Collider other)
    {
        var rigidbody = other.attachedRigidbody;

        if (rigidbody == null)
        {
            return true;
        }

        var proxy = rigidbody.GetComponent<TriggerRefProxy>();

        if (proxy == null)
        {
            return true;
        }

        var root = proxy.root;

        if (root == null)
        {
            return true;
        }

        var rig = RigManager.Cache.Get(root);

        if (rig == null)
        {
            return true;
        }

        if (!rig.IsLocalPlayer())
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GenGameControl_Trigger.OnTriggerEnter))]
    public static bool OnTriggerEnter(GenGameControl_Trigger __instance, Collider other)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        return ProcessTrigger(__instance, other);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GenGameControl_Trigger.OnTriggerExit))]
    public static bool OnTriggerExit(GenGameControl_Trigger __instance, Collider other)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        return ProcessTrigger(__instance, other);
    }
}