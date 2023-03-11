using HarmonyLib;

using System;
using System.Collections.Generic;

using UnityEngine;

using LabFusion.Utilities;

using SLZ.AI;
using SLZ.Rig;

using LabFusion.SDK.Points;
using LabFusion.Representation;
using LabFusion.Network;
using LabFusion.MonoBehaviours;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Mirror))]
    public static class MirrorPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Mirror.OnTriggerEnter))]
        public static bool OnTriggerEnter(Mirror __instance, Collider c)
        {
            if (c.CompareTag("Player")) {
                if (NetworkInfo.HasServer)
                    return OnEnterMultiplayer(__instance, c);
                else
                    return OnEnterSingleplayer(__instance, c);
            }

            return true;
        }

        private static bool OnEnterSingleplayer(Mirror __instance, Collider c) {
            var rigManager = RigManager.Cache.Get(TriggerRefProxy.Cache.Get(c.gameObject).root);

            foreach (var item in PointItemManager.LoadedItems) {
                if (item.IsEquipped)
                {
                    item.OnUpdateObjects(new PointItemPayload()
                    {
                        type = PointItemPayloadType.MIRROR,
                        rigManager = rigManager,
                        mirror = __instance,
                        playerId = PlayerIdManager.LocalId,
                    }, true);
                }
            }

            return true;
        }

        private static bool OnEnterMultiplayer(Mirror __instance, Collider c) {
            // Check if we have a identifier
            RigManager rig = null;
            PlayerId playerId;

            // If we do, get the rig manager and id
            var identifier = __instance.GetComponent<MirrorIdentifier>();
            if (identifier != null)
            {
                playerId = PlayerIdManager.GetPlayerId(identifier.id);

                if (playerId != null && PlayerRepUtilities.TryGetReferences(playerId, out var references))
                    rig = references.RigManager;
            }
            // Otherwise, clone the mirror and setup IDs
            else
            {
                // Get trigger ref proxy
                var triggerRef = TriggerRefProxy.Cache.Get(c.gameObject);
                if (triggerRef == null || triggerRef.root == null)
                    return true;

                if (!PlayerRepUtilities.TryGetRigInfo(RigManager.Cache.Get(triggerRef.root), out byte targetId, out _))
                    return true;

                // Add identifiers
                identifier = __instance.gameObject.AddComponent<MirrorIdentifier>();
                byte localId = PlayerIdManager.LocalSmallId;
                identifier.id = localId;

                Transform root = new GameObject("Fusion Mirror Root").transform;
                root.gameObject.SetActive(false);
                root.transform.parent = __instance.transform.parent;
                root.gameObject.AddComponent<DestroyOnDisconnect>();

                for (byte i = 0; i < 5; i++)
                {
                    if (i == localId)
                        i++;

                    Transform cloneRoot = new GameObject($"Mirror {i}").transform;
                    cloneRoot.parent = root;

                    var clone = GameObject.Instantiate(__instance.gameObject, cloneRoot, true);
                    clone.name = __instance.gameObject.name;

                    clone.GetComponent<MirrorIdentifier>().id = i;

                    var newMirror = clone.GetComponent<Mirror>();
                    var newReflectTran = GameObject.Instantiate(newMirror._reflectTran.gameObject, cloneRoot, true);

                    newReflectTran.gameObject.name = __instance._reflectTran.name;

                    newMirror._reflectTran = newReflectTran.transform;
                    newMirror._avatarsTran = newReflectTran.transform.Find("AVATARS");
                }

                root.gameObject.SetActive(true);

                // Get values
                if (identifier.id != targetId)
                    return false;

                playerId = PlayerIdManager.GetPlayerId(identifier.id);

                if (playerId != null && PlayerRepUtilities.TryGetReferences(playerId, out var references))
                    rig = references.RigManager;
            }

            if (rig == null || playerId == null)
                return false;

            bool isTarget = TriggerUtilities.IsMatchingRig(c, rig);

            if (isTarget)
            {
                foreach (var item in PointItemManager.LoadedItems)
                {
                    if (playerId.EquippedItems.Contains(item.Barcode))
                    {
                        item.OnUpdateObjects(new PointItemPayload()
                        {
                            type = PointItemPayloadType.MIRROR,
                            rigManager = rig,
                            mirror = __instance,
                            playerId = playerId,
                        }, true);
                    }
                }
            }

            return isTarget;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Mirror.OnTriggerExit))]
        public static bool OnTriggerExit(Mirror __instance, Collider c)
        {
            if (c.CompareTag("Player")) {
                if (NetworkInfo.HasServer)
                    return OnExitMultiplayer(__instance, c);
                else
                    return OnExitSingleplayer(__instance, c);
            }


            return true;
        }

        private static bool OnExitMultiplayer(Mirror __instance, Collider c) {
            // Check if we have a identifier
            RigManager rig = null;
            PlayerId playerId = null;

            // If we do, get the rig manager and id
            var identifier = __instance.GetComponent<MirrorIdentifier>();
            if (identifier != null)
            {
                playerId = PlayerIdManager.GetPlayerId(identifier.id);

                if (playerId != null && PlayerRepUtilities.TryGetReferences(playerId, out var references))
                    rig = references.RigManager;
            }

            if (rig == null || playerId == null)
                return false;

            bool isTarget = TriggerUtilities.IsMatchingRig(c, rig);

            if (isTarget)
            {
                foreach (var item in PointItemManager.LoadedItems)
                {
                    if (playerId.EquippedItems.Contains(item.Barcode))
                    {
                        item.OnUpdateObjects(new PointItemPayload()
                        {
                            type = PointItemPayloadType.MIRROR,
                            rigManager = rig,
                            mirror = __instance,
                            playerId = playerId,
                        }, false);
                    }
                }
            }

            return isTarget;
        }

        private static bool OnExitSingleplayer(Mirror __instance, Collider c) {
            var rigManager = RigManager.Cache.Get(TriggerRefProxy.Cache.Get(c.gameObject).root);

            foreach (var item in PointItemManager.LoadedItems) {
                if (item.IsEquipped) {
                    item.OnUpdateObjects(new PointItemPayload()
                    {
                        type = PointItemPayloadType.MIRROR,
                        rigManager = rigManager,
                        mirror = __instance,
                        playerId = PlayerIdManager.LocalId,
                    }, false);
                }
            }


            return true;
        }
    }

}
