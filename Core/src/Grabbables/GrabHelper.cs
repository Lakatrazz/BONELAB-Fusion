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
                MelonCoroutines.Start(Internal_ObjectForcePullRoutine(hand, grip));
            }
        }

        internal static IEnumerator Internal_ObjectForcePullRoutine(Hand hand, Grip grip) {
            // Delay a few frames
            for (var i = 0; i < 4; i++)
                yield return null;

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
                        while (!syncable.IsRegistered())
                            yield return null;

                        PropSender.SendOwnershipTransfer(syncable);
                    }
                    // Create a new one
                    else if (!NetworkInfo.IsServer) {
                        // Create this as a syncable
                        syncable = new PropSyncable(host);
                        syncable.SetOwner(PlayerIdManager.LocalSmallId);

                        // Add it to the queue and get a unique id
                        ushort queuedId = SyncManager.QueueSyncable(syncable);

                        using (var writer = FusionWriter.Create()) {
                            using (var data = SyncableIDRequestData.Create(smallId, queuedId)) {
                                writer.Write(data);

                                using (var message = FusionMessage.Create(NativeMessageTag.SyncableIDRequest, writer)) {
                                    MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                                }
                            }
                        }

                        while (syncable.IsQueued())
                            yield return null;

                        yield return null;

                        // Send force grab message
                        var grab = new SerializedPropGrab(host.gameObject.GetFullPath(), syncable.GetIndex(grip).Value, syncable.Id, new GripPair(hand, grip));

                        using (var writer = FusionWriter.Create()) {
                            using (var data = PlayerRepForceGrabData.Create(smallId, grab)) {
                                writer.Write(data);

                                using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepForceGrab, writer)) {
                                    MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                    else if (NetworkInfo.IsServer)
                    {
                        // Add new syncable and send force grab message
                        syncable = new PropSyncable(host);
                        SyncManager.RegisterSyncable(syncable, SyncManager.AllocateSyncID());
                        var grab = new SerializedPropGrab(host.gameObject.GetFullPath(), syncable.GetIndex(grip).Value, syncable.Id, new GripPair(hand, grip));

                        using (var writer = FusionWriter.Create()) {
                            using (var data = PlayerRepForceGrabData.Create(smallId, grab)) {
                                writer.Write(data);

                                using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepForceGrab, writer)) {
                                    MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void SendObjectAttach(Hand hand, Grip grip)
        {
            if (NetworkInfo.HasServer)
            {
                MelonCoroutines.Start(Internal_ObjectAttachRoutine(hand, grip));
            }
        }

        internal static IEnumerator Internal_ObjectAttachRoutine(Hand hand, Grip grip)
        {
            // Delay a few frames
            for (var i = 0; i < 4; i++)
                yield return null;

            if (NetworkInfo.HasServer)
            {
                var handedness = hand.handedness;

                // Get base values for the message
                byte smallId = PlayerIdManager.LocalSmallId;
                GrabGroup group = GrabGroup.UNKNOWN;
                SerializedGrab serializedGrab = null;
                bool validGrip = false;

                // If the grip exists, we'll check its stuff
                if (grip != null)
                {
                    // Check for player body grab
                    if (PlayerRepUtilities.FindAttachedPlayer(grip, out var repId, out var repReferences))
                    {
                        group = GrabGroup.PLAYER_BODY;
                        serializedGrab = new SerializedPlayerBodyGrab(repId, repReferences.GetIndex(grip).Value, new GripPair(hand, grip));
                        validGrip = true;
                    }
                    // Check for static grips
                    else if (grip.IsStatic)
                    {
                        if (grip.TryCast<WorldGrip>() != null)
                        {
                            group = GrabGroup.WORLD_GRIP;
                            serializedGrab = new SerializedWorldGrab(smallId);
                            validGrip = true;
                        }
                        else
                        {
                            group = GrabGroup.STATIC;
                            serializedGrab = new SerializedStaticGrab(grip.gameObject.GetFullPath());
                            validGrip = true;
                        }
                    }
                    // Check for prop grips
                    else if (grip.HasRigidbody && !grip.GetComponentInParent<RigManager>())
                    {
                        group = GrabGroup.PROP;
                        GetGripInfo(grip, out var host);

                        GameObject root = host.GetSyncRoot();

                        // Do we already have a synced object?
                        if (GripExtender.Cache.TryGet(grip, out var syncable) || PropSyncable.HostCache.TryGet(host.gameObject, out syncable) || PropSyncable.Cache.TryGet(root, out syncable))
                        {
                            serializedGrab = new SerializedPropGrab("_", syncable.GetIndex(grip).Value, syncable.GetId(), new GripPair(hand, grip));
                            validGrip = true;
                        }
                        // Create a new one
                        else if (!NetworkInfo.IsServer)
                        {
                            syncable = new PropSyncable(host);

                            ushort queuedId = SyncManager.QueueSyncable(syncable);

                            using (var writer = FusionWriter.Create())
                            {
                                using (var data = SyncableIDRequestData.Create(smallId, queuedId))
                                {
                                    writer.Write(data);

                                    using (var message = FusionMessage.Create(NativeMessageTag.SyncableIDRequest, writer))
                                    {
                                        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                                    }
                                }
                            }

                            while (syncable.IsQueued())
                                yield return null;

                            yield return null;

                            serializedGrab = new SerializedPropGrab(host.gameObject.GetFullPath(), syncable.GetIndex(grip).Value, syncable.Id, new GripPair(hand, grip));
                            validGrip = true;
                        }
                        else if (NetworkInfo.IsServer)
                        {
                            syncable = new PropSyncable(host);
                            SyncManager.RegisterSyncable(syncable, SyncManager.AllocateSyncID());
                            serializedGrab = new SerializedPropGrab(host.gameObject.GetFullPath(), syncable.GetIndex(grip).Value, syncable.Id, new GripPair(hand, grip));

                            validGrip = true;
                        }
                    }
                }

                // Now, send the message
                if (validGrip) {
                    // Set whether or not this is still currently grabbed
                    serializedGrab.isGrabbed = hand.m_CurrentAttachedGO == grip.gameObject;

                    using (var writer = FusionWriter.Create())
                    {
                        using (var data = PlayerRepGrabData.Create(smallId, handedness, group, serializedGrab))
                        {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepGrab, writer))
                            {
                                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
        }

        public static void SendObjectDetach(Hand hand)
        {
            MelonCoroutines.Start(Internal_ObjectDetachRoutine(hand));
        }

        internal static IEnumerator Internal_ObjectDetachRoutine(Hand hand) {
            // Delay a few frames
            for (var i = 0; i < 8; i++)
                yield return null;

            var handedness = hand.handedness;

            if (hand.m_CurrentAttachedGO != null)
                yield break;

            if (NetworkInfo.HasServer)
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = PlayerRepReleaseData.Create(PlayerIdManager.LocalSmallId, handedness))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepRelease, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
