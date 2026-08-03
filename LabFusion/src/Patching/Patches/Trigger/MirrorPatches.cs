using HarmonyLib;

using UnityEngine;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.AI;

using LabFusion.SDK.Points;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.MonoBehaviours;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.SDK.Wearables;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(Mirror))]
public static class MirrorPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Mirror.WriteTransforms))]
    public static void WriteTransforms(Mirror __instance)
    {
        if (!NetworkManager.HasServer)
        {
            return;
        }

        var playerJaw = __instance.rigManager.avatar.animator.GetBoneTransform(HumanBodyBones.Jaw);

        if (playerJaw == null)
        {
            return;
        }

        var reflectionJaw = __instance.Reflection.animator.GetBoneTransform(HumanBodyBones.Jaw);

        if (reflectionJaw == null)
        {
            return;
        }

        reflectionJaw.localRotation = playerJaw.localRotation;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Mirror.OnTriggerEnter))]
    public static bool OnTriggerEnter(Mirror __instance, Collider c)
    {
        var rb = c.attachedRigidbody;
        if (!rb)
        {
            return true;
        }

        var triggerRefProxy = rb.GetComponent<TriggerRefProxy>();
        if (!triggerRefProxy || triggerRefProxy.triggerType != TriggerRefProxy.TriggerType.Player)
        {
            return true;
        }

        if (NetworkManager.HasServer)
            return OnEnterMultiplayer(__instance, triggerRefProxy);
        else
            return OnEnterSingleplayer(__instance, triggerRefProxy);
    }

    private static bool OnEnterSingleplayer(Mirror __instance, TriggerRefProxy proxy)
    {
        WearableManager.LocalDisplayer.AddReflectionOrigin(__instance._reflectTran);
        return true;
    }

    private static bool OnEnterMultiplayer(Mirror __instance, TriggerRefProxy proxy)
    {
        // Check if we have a identifier
        RigManager rig = null;
        PlayerID playerID;

        // If we do, get the rig manager and id
        var identifier = __instance.GetComponent<MirrorIdentifier>();
        if (identifier != null)
        {
            playerID = PlayerIDManager.GetPlayerID(identifier.ID);

            if (playerID != null && NetworkPlayerManager.TryGetPlayer(playerID.SmallID, out var player))
            {
                rig = player.NetworkRig.RigRefs.RigManager;
            }
        }
        // Otherwise, clone the mirror and setup IDs
        else
        {
            bool hasPlayer = NetworkPlayerManager.TryGetPlayer(RigManager.Cache.Get(proxy.root), out var player);

            if (!hasPlayer)
            {
                return true;
            }

            ClientSmallID targetId = player.PlayerID.SmallID;

            // Add identifiers
            identifier = __instance.gameObject.AddComponent<MirrorIdentifier>();
            ClientSmallID localId = PlayerIDManager.LocalSmallID;
            identifier.ID = localId;

            Transform root = new GameObject("Fusion Mirror Root").transform;
            root.gameObject.SetActive(false);
            root.transform.parent = __instance.transform.parent;
            root.gameObject.AddComponent<DestroyOnDisconnect>();

            for (byte i = 0; i < 5; i++)
            {
                if (i == (byte)localId)
                {
                    i++;
                }

                Transform cloneRoot = new GameObject($"Mirror {i}").transform;
                cloneRoot.parent = root;

                var clone = GameObject.Instantiate(__instance.gameObject, cloneRoot, true);
                clone.name = __instance.gameObject.name;

                clone.GetComponent<MirrorIdentifier>().ID = new(i);

                var newMirror = clone.GetComponent<Mirror>();
                var newReflectTran = GameObject.Instantiate(newMirror._reflectTran.gameObject, cloneRoot, true);

                newReflectTran.gameObject.name = __instance._reflectTran.name;

                newMirror._reflectTran = newReflectTran.transform;
                newMirror._avatarsTran = newReflectTran.transform.Find("AVATARS");
            }

            root.gameObject.SetActive(true);

            // Get values
            if (identifier.ID != targetId)
            {
                return false;
            }

            playerID = PlayerIDManager.GetPlayerID(identifier.ID);

            if (playerID != null && NetworkPlayerManager.TryGetPlayer(playerID.SmallID, out var identifiedPlayer))
            {
                rig = identifiedPlayer.NetworkRig.RigRefs.RigManager;
            }
        }

        if (rig == null || playerID == null)
            return false;

        bool isTarget = TriggerUtilities.IsMatchingRig(proxy, rig);

        if (isTarget)
        {
            var wearableDisplayer = WearableManager.GetWearableDisplayer(rig);

            if (wearableDisplayer != null)
            {
                wearableDisplayer.AddReflectionOrigin(__instance._reflectTran);
            }
        }

        return isTarget;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Mirror.OnTriggerExit))]
    public static bool OnTriggerExit(Mirror __instance, Collider c)
    {
        var rb = c.attachedRigidbody;
        if (!rb)
        {
            return true;
        }

        var triggerRefProxy = rb.GetComponent<TriggerRefProxy>();
        if (!triggerRefProxy || triggerRefProxy.triggerType != TriggerRefProxy.TriggerType.Player)
        {
            return true;
        }

        if (NetworkManager.HasServer)
            return OnExitMultiplayer(__instance, triggerRefProxy);
        else
            return OnExitSingleplayer(__instance, triggerRefProxy);
    }

    private static bool OnExitMultiplayer(Mirror __instance, TriggerRefProxy proxy)
    {
        // Check if we have a identifier
        RigManager rig = null;
        PlayerID playerId = null;

        // If we do, get the rig manager and id
        var identifier = __instance.GetComponent<MirrorIdentifier>();
        if (identifier != null)
        {
            playerId = PlayerIDManager.GetPlayerID(identifier.ID);

            if (playerId != null && NetworkPlayerManager.TryGetPlayer(playerId.SmallID, out var identifiedPlayer))
            {
                rig = identifiedPlayer.NetworkRig.RigRefs.RigManager;
            }
        }

        if (rig == null || playerId == null)
            return false;

        bool isTarget = TriggerUtilities.IsMatchingRig(proxy, rig);

        if (isTarget)
        {
            var wearableDisplayer = WearableManager.GetWearableDisplayer(rig);

            if (wearableDisplayer != null)
            {
                wearableDisplayer.RemoveReflectionOrigin(__instance._reflectTran);
            }
        }

        return isTarget;
    }

    private static bool OnExitSingleplayer(Mirror __instance, TriggerRefProxy proxy)
    {
        WearableManager.LocalDisplayer.RemoveReflectionOrigin(__instance._reflectTran);

        return true;
    }
}