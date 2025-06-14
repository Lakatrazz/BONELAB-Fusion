using HarmonyLib;

using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Player;
using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Scene;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(Grip))]
public static class GripPatches
{
    public static readonly ComponentHashTable<Grip> HashTable = new();

    private static bool HasMarrowEntity(Grip grip)
    {
        if (grip._marrowEntity != null)
        {
            return true;
        }

        // Sometimes grips don't have their marrow entity set yet on Awake, but the InteractableHost does
        var host = grip.GetComponentInParent<InteractableHost>();

        if (host != null && host.marrowEntity != null)
        {
            return true;
        }

        return false;
    }

    private static bool IsRuntimeCreated(Grip grip)
    {
        if (grip.TryCast<WorldGrip>() != null)
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(nameof(Grip.Awake))]
    [HarmonyPrefix]
    private static void Awake(Grip __instance)
    {
        // Only hash grips which don't have an entity (static grips)
        if (HasMarrowEntity(__instance))
        {
            return;
        }

        // Don't hash runtime created grips either (ex. world grips)
        if (IsRuntimeCreated(__instance))
        {
            return;
        }

        var hash = GameObjectHasher.GetHierarchyHash(__instance.gameObject);

        var index = HashTable.AddComponent(hash, __instance);

#if DEBUG
        if (index > 0)
        {
            FusionLogger.Log($"Grip {__instance.name} had a conflicting hash {hash} and has been added at index {index}.");
        }
#endif
    }

    [HarmonyPatch(nameof(Grip.OnDestroy))]
    [HarmonyPrefix]
    private static void OnDestroy(Grip __instance)
    {
        HashTable.RemoveComponent(__instance);
    }

    [HarmonyPatch(nameof(Grip.OnAttachedToHand))]
    [HarmonyPostfix]
    private static void OnAttachedToHand(Grip __instance, Hand hand)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        // Make sure this is the local player
        if (!hand.manager.IsLocalPlayer())
        {
            return;
        }

        GrabHelper.SendObjectAttach(hand, __instance);

        try
        {
            LocalPlayer.OnGrab?.Invoke(hand, __instance);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("running LocalPlayer.OnGrab", e);
        }
    }

    [HarmonyPatch(nameof(Grip.OnDetachedFromHand))]
    [HarmonyPostfix]
    private static void OnDetachedFromHand(Grip __instance, Hand hand)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var rigManager = hand.manager;

        if (rigManager.IsLocalPlayer())
        {
            OnDetachedFromLocalPlayer(__instance, hand);
        }
        else if (NetworkPlayerManager.TryGetPlayer(rigManager, out var networkPlayer))
        {
            OnDetachedFromNetworkPlayer(__instance, hand, networkPlayer);
        }
    }

    private static void OnDetachedFromLocalPlayer(Grip grip, Hand hand)
    {
        GrabHelper.SendObjectDetach(hand);

        try
        {
            LocalPlayer.OnRelease?.Invoke(hand, grip);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("running LocalPlayer.OnRelease", e);
        }
    }

    private static void OnDetachedFromNetworkPlayer(Grip grip, Hand hand, NetworkPlayer networkPlayer)
    {
        try
        {
            networkPlayer.Grabber.CheckDetachAndReattach(hand, grip);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("running RigGrabber.CheckDetachAndReattach", e);
        }
    }
}