using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLZ.Rig;

using UnhollowerRuntimeLib;

using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Extensions;

using SLZ;
using SLZ.Interaction;
using MelonLoader;

namespace LabFusion.Representation {
    public static class PlayerRepUtilities {
        public const int TransformSyncCount = 3;

        public const string PolyBlankBarcode = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

        public static bool FindAttachedPlayerRep(Grip grip, out PlayerRep rep) {
            rep = null;

            if (grip == null)
                return false;

            var rig = RigManager.Cache.Get(grip.transform.root.gameObject);
            if (rig && PlayerRep.Managers.ContainsKey(rig))
            {
                rep = PlayerRep.Managers[rig];
                return true;
            }
            else {
                return false;
            }
        }

        public static void SendObjectAttach(Handedness handedness, Grip grip) {
            if (NetworkInfo.HasServer) {
                MelonCoroutines.Start(Internal_ObjectAttachRoutine(handedness, grip));
            }
        }

        internal static IEnumerator Internal_ObjectAttachRoutine(Handedness handedness, Grip grip) { 
            if (NetworkInfo.HasServer) {
                // Get base values for the message
                byte smallId = PlayerIdManager.LocalSmallId;
                SyncUtilities.SyncGroup group = SyncUtilities.SyncGroup.UNKNOWN;
                SerializedGrab serializedGrab = null;
                bool validGrip = false;

                // If the grip exists, we'll check its stuff
                if (grip != null) {
                    // Check for player body grab
                    if (FindAttachedPlayerRep(grip, out var rep)) {
#if DEBUG
                        FusionLogger.Log("Found player rep grip!");
#endif

                        group = SyncUtilities.SyncGroup.PLAYER_BODY;
                        serializedGrab = new SerializedPlayerBodyGrab(rep.PlayerId.SmallId, rep.RigReferences.GetIndex(grip).Value);
                        validGrip = true;
                    }
                    // Check for static grips
                    else if (grip.IsStatic) {
#if DEBUG
                        FusionLogger.Log("Found grip with no rigidbody!");
#endif

                        if (grip.TryCast<WorldGrip>() != null) {
                            group = SyncUtilities.SyncGroup.WORLD_GRIP;
                            serializedGrab = new SerializedWorldGrab(smallId, new SerializedTransform(grip.transform));
                            validGrip = true;
                        }
                        else {
                            group = SyncUtilities.SyncGroup.STATIC;
                            serializedGrab = new SerializedStaticGrab(grip.transform.GetPath());
                            validGrip = true;
                        }
                    }
                    // Check for prop grips
                    else if (grip.HasRigidbody && !grip.GetComponentInParent<RigManager>()) {
                        group = SyncUtilities.SyncGroup.PROP;
                        var go = grip.Host.Rb.gameObject;
                        
                        // Do we already have a synced object?
                        if (PropSyncable.Cache.TryGetValue(go, out var syncable)) {
                            serializedGrab = new SerializedPropGrab("_", syncable.GetIndex(grip).Value, syncable.GetId(), true);
                            validGrip = true;
                        }
                        // Create a new one
                        else if (!NetworkInfo.IsServer) {
                            syncable = new PropSyncable(go);

                            ushort queuedId = SyncUtilities.QueueSyncable(syncable);

                            using (var writer = FusionWriter.Create()) {
                                using (var data = SyncableIDRequestData.Create(smallId, queuedId)) {
                                    writer.Write(data);

                                    using (var message = FusionMessage.Create(NativeMessageTag.SyncableIDRequest, writer)) {
                                        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                                    }
                                }
                            }

                            while (syncable.IsQueued())
                                yield return null;

                            yield return null;

#if DEBUG
                            FusionLogger.Log($"Sending new grab message with an id of {syncable.Id}");
#endif

                            serializedGrab = new SerializedPropGrab(grip.Host.Rb.transform.GetPath(), syncable.GetIndex(grip).Value, syncable.Id, true);
                            validGrip = true;
                        }
                        else if (NetworkInfo.IsServer) {
                            syncable = new PropSyncable(go);
                            SyncUtilities.RegisterSyncable(syncable, SyncUtilities.AllocateSyncID());
                            serializedGrab = new SerializedPropGrab(grip.Host.Rb.transform.GetPath(), syncable.GetIndex(grip).Value, syncable.Id, true);

                            validGrip = true;
                        }
                    }
                    // Nothing left
                    else {
#if DEBUG
                        FusionLogger.Log("Found no valid grip for syncing!");
#endif
                    }
                }

                // Now, send the message
                if (validGrip) {
                    using (var writer = FusionWriter.Create()) {
                        using (var data = PlayerRepGrabData.Create(smallId, handedness, group, serializedGrab)) {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepGrab, writer)) {
                                MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
        }

        public static void SendObjectDetach(Handedness handedness) {
            if (NetworkInfo.HasServer) {
                using (var writer = FusionWriter.Create()) {
                    using (var data = PlayerRepReleaseData.Create(PlayerIdManager.LocalSmallId, handedness)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepRelease, writer)) {
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static RigManager CreateNewRig() {
            var go = GameObject.Instantiate(AssetBundleManager.PlayerRepBundle.LoadAsset(ResourcePaths.PlayerRepName, Il2CppType.Of<GameObject>())).Cast<GameObject>();

            if (RigData.RigReferences.RigManager) {
                go.transform.position = RigData.RigSpawn;
                go.transform.rotation = RigData.RigSpawnRot;
            }

            return go.GetComponent<RigManager>();
        }

        public static void FillTransformArray(ref Transform[] array, RigManager manager) {
            var rig = manager.openControllerRig;

            array[0] = rig.m_head;
            array[1] = rig.m_handLf;
            array[2] = rig.m_handRt;
        }
    }
}
