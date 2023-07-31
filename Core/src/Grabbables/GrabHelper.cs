using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLZ.Rig;
using SLZ.Marrow.Pool;

using UnhollowerRuntimeLib;

using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Syncables;
using LabFusion.Extensions;
using LabFusion.Representation;

using SLZ;
using SLZ.Interaction;

using MelonLoader;
using LabFusion.Senders;

namespace LabFusion.Grabbables {
    public static class GrabHelper {
        public static void GetGripInfo(Grip grip, out InteractableHost host) {
            host = null;

            if (grip == null)
                return;

            var iGrippable = grip.Host;

            InteractableHost interactableHost;
            if (interactableHost = iGrippable.TryCast<InteractableHost>()) {
                // Try and find the host and manager
                host = interactableHost;
                var manager = host.manager;

                // Loop through the cache of all parents if there is no manager
                if (manager == null) {
                    Transform parent = host.transform.parent;

                    while (parent != null) {
                        var foundHost = InteractableHost.Cache.Get(parent.gameObject);

                        if (foundHost != null)
                            host = foundHost;

                        parent = parent.parent;
                    }
                }
            }
        }

        public static void SendObjectForcePull(Hand hand, Grip grip) {
            if (NetworkInfo.HasServer) {
                DelayUtilities.Delay(() => {
                    Internal_ObjectForcePull(hand, grip);
                }, 4);
            }
        }

        internal static void Internal_ObjectForcePull(Hand hand, Grip grip) {
            if (NetworkInfo.HasServer) {
                // Check to see if this has a rigidbody
                if (grip.HasRigidbody && !grip.GetComponentInParent<RigManager>())
                {
                    // Get base values for the message
                    byte smallId = PlayerIdManager.LocalSmallId;

                    GetGripInfo(grip, out var host);

                    GameObject root = host.GetSyncRoot();

                    // Do we already have a synced object?
                    if (GripExtender.Cache.TryGet(grip, out var syncable) || PropSyncable.HostCache.TryGet(host.gameObject, out syncable) || PropSyncable.Cache.TryGet(root, out syncable)) {
                        syncable.HookOnRegistered(() => {
                            PropSender.SendOwnershipTransfer(syncable);
                        });
                    }
                    // Create a new one
                    else {
                        PropSender.SendPropCreation(root, null, false);
                    }
                }
            }
        }

        public static void SendObjectAttach(Hand hand, Grip grip)
        {
            if (NetworkInfo.HasServer)
            {
                DelayUtilities.Delay(() => {
                    Internal_ObjectAttach(hand, grip);
                }, 4);
            }
        }

        internal static void Internal_ObjectAttach(Hand hand, Grip grip)
        {
            if (NetworkInfo.HasServer)
            {
                var handedness = hand.handedness;

                // Get base values for the message
                byte smallId = PlayerIdManager.LocalSmallId;
                GrabGroup group = GrabGroup.UNKNOWN;
                SerializedGrab serializedGrab = null;

                // If the grip exists, we'll check its stuff
                if (grip != null)
                {
                    // Check for player body grab
                    if (PlayerRepUtilities.FindAttachedPlayer(grip, out var repId, out var repReferences, out var isAvatarGrip))
                    {
                        group = GrabGroup.PLAYER_BODY;
                        serializedGrab = new SerializedPlayerBodyGrab(repId, repReferences.GetIndex(grip, isAvatarGrip).Value, isAvatarGrip);
                        OnFinish();
                    }
                    // Check for static grips
                    else if (grip.IsStatic) {
                        if (grip.TryCast<WorldGrip>() != null) {
                            group = GrabGroup.WORLD_GRIP;
                            serializedGrab = new SerializedWorldGrab(smallId);
                            OnFinish();
                        }
                        else {
                            group = GrabGroup.STATIC;

                            _ = grip.gameObject.GetFullPathAsync((p) => {
                                serializedGrab = new SerializedStaticGrab(p);
                                OnFinish();
                            });
                        }
                    }
                    // Check for prop grips
                    else if (grip.HasRigidbody && !grip.GetComponentInParent<RigManager>())
                    {
                        group = GrabGroup.PROP;
                        GetGripInfo(grip, out var host);

                        GameObject root = host.GetSyncRoot();

                        // Do we already have a synced object?
                        if (GripExtender.Cache.TryGet(grip, out var syncable) || PropSyncable.HostCache.TryGet(host.gameObject, out syncable) || PropSyncable.Cache.TryGet(root, out syncable)) {
                            serializedGrab = new SerializedPropGrab("_", syncable.GetIndex(grip).Value, syncable.GetId());
                            OnFinish();
                        }
                        else {
                            // Make sure the GameObject is whitelisted before syncing
                            if (!root.IsSyncWhitelisted())
                                return;

                            // Invoked when a PropSyncable is finished gathering its pathed
                            void OnPropFinish(PropSyncable syncable, string path) {
                                serializedGrab = new SerializedPropGrab(path, syncable.GetIndex(grip).Value, syncable.Id);
                                OnFinish();
                            }

                            // Create a new one
                            if (!NetworkInfo.IsServer) {
                                syncable = new PropSyncable(host);
    
                                ushort queuedId = SyncManager.QueueSyncable(syncable);
    
                                using (var writer = FusionWriter.Create(SyncableIDRequestData.Size)) {
                                    using var data = SyncableIDRequestData.Create(smallId, queuedId);
                                    writer.Write(data);

                                    using var message = FusionMessage.Create(NativeMessageTag.SyncableIDRequest, writer);
                                    MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                                }
    
                                syncable.HookOnRegistered(() => {
                                    _ = host.gameObject.GetFullPathAsync((p) => { OnPropFinish(syncable, p); });
                                });
                            }
                            else if (NetworkInfo.IsServer)
                            {
                                syncable = new PropSyncable(host);
                                SyncManager.RegisterSyncable(syncable, SyncManager.AllocateSyncID());

                                _ = host.gameObject.GetFullPathAsync((p) => { OnPropFinish(syncable, p); });
                            }
                        }
                    }
                }

                // Send the message when whatever task is finished
                void OnFinish() {
                    // Write the default grip values
                    serializedGrab.WriteDefaultGrip(hand, grip);

                    using var writer = FusionWriter.Create(PlayerRepGrabData.Size + serializedGrab.GetSize());
                    using var data = PlayerRepGrabData.Create(smallId, handedness, group, serializedGrab);
                    writer.Write(data);

                    using var message = FusionMessage.Create(NativeMessageTag.PlayerRepGrab, writer);
                    MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                }
            }
        }

        public static void SendObjectDetach(Hand hand)
        {
            DelayUtilities.Delay(() =>
            {
                Internal_ObjectDetach(hand);
            }, 8);
        }

        internal static void Internal_ObjectDetach(Hand hand) {
            var handedness = hand.handedness;

            if (hand.m_CurrentAttachedGO != null)
                return;

            if (NetworkInfo.HasServer)
            {
                using var writer = FusionWriter.Create(PlayerRepReleaseData.Size);
                using var data = PlayerRepReleaseData.Create(PlayerIdManager.LocalSmallId, handedness);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PlayerRepRelease, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
        }
    }
}
