using HarmonyLib;

using UnityEngine;

using LabFusion.Utilities;

using Il2CppSLZ.VRMK;
using Il2CppSLZ.Rig;
using Il2CppSLZ.Marrow.AI;

using LabFusion.SDK.Points;
using LabFusion.Representation;
using LabFusion.Network;
using LabFusion.MonoBehaviours;
using static Il2CppSLZ.Marrow.PuppetMasta.Muscle;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Mirror))]
    public static class MirrorPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Mirror.WriteTransforms))]
        public static void WriteTransforms(Mirror __instance)
        {
            if (!NetworkInfo.HasServer)
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

            if (NetworkInfo.HasServer)
                return OnEnterMultiplayer(__instance, triggerRefProxy);
            else
                return OnEnterSingleplayer(__instance, triggerRefProxy);
        }

        private static bool OnEnterSingleplayer(Mirror __instance, TriggerRefProxy proxy)
        {
            var rigManager = RigManager.Cache.Get(proxy.root);

            foreach (var item in PointItemManager.LoadedItems)
            {
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

        private static bool OnEnterMultiplayer(Mirror __instance, TriggerRefProxy proxy)
        {
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
                if (!PlayerRepUtilities.TryGetRigInfo(RigManager.Cache.Get(proxy.root), out byte targetId, out _))
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

            bool isTarget = TriggerUtilities.IsMatchingRig(proxy, rig);

            if (isTarget)
            {
                foreach (var item in PointItemManager.LoadedItems)
                {
                    if (playerId.HasEquipped(item))
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

            if (NetworkInfo.HasServer)
                return OnExitMultiplayer(__instance, triggerRefProxy);
            else
                return OnExitSingleplayer(__instance, triggerRefProxy);
        }

        private static bool OnExitMultiplayer(Mirror __instance, TriggerRefProxy proxy)
        {
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

            bool isTarget = TriggerUtilities.IsMatchingRig(proxy, rig);

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

        private static bool OnExitSingleplayer(Mirror __instance, TriggerRefProxy proxy)
        {
            var rigManager = RigManager.Cache.Get(proxy.root);

            foreach (var item in PointItemManager.LoadedItems)
            {
                if (item.IsEquipped)
                {
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
