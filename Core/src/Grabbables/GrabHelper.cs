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
using LabFusion.Syncables;
using LabFusion.Extensions;
using LabFusion.Representation;

using SLZ;
using SLZ.Interaction;
using MelonLoader;

namespace LabFusion.Grabbables {
    public static class GrabHelper {
        public static void GetGripInfo(Grip grip, out GameObject root, out Grip[] otherGrips) {
            root = null;
            otherGrips = null;

            if (grip == null)
                return;

            var iGrippable = grip.Host;
            root = iGrippable.GetHostGameObject();

            InteractableHost interactableHost;
            if (interactableHost = iGrippable.TryCast<InteractableHost>()) {
                otherGrips = (Grip[])interactableHost._grips.ToArray();
            }
        }

        public static void SendObjectAttach(Handedness handedness, Grip grip)
        {
            if (NetworkInfo.HasServer)
            {
                MelonCoroutines.Start(Internal_ObjectAttachRoutine(handedness, grip));
            }
        }

        internal static IEnumerator Internal_ObjectAttachRoutine(Handedness handedness, Grip grip)
        {
            if (NetworkInfo.HasServer)
            {
                // Get base values for the message
                byte smallId = PlayerIdManager.LocalSmallId;
                GrabGroup group = GrabGroup.UNKNOWN;
                SerializedGrab serializedGrab = null;
                bool validGrip = false;

                // If the grip exists, we'll check its stuff
                if (grip != null)
                {
                    // Check for player body grab
                    if (PlayerRepUtilities.FindAttachedPlayerRep(grip, out var rep))
                    {
#if DEBUG
                        FusionLogger.Log("Found player rep grip!");
#endif

                        group = GrabGroup.PLAYER_BODY;
                        serializedGrab = new SerializedPlayerBodyGrab(rep.PlayerId.SmallId, rep.RigReferences.GetIndex(grip).Value);
                        validGrip = true;
                    }
                    // Check for static grips
                    else if (grip.IsStatic)
                    {
#if DEBUG
                        FusionLogger.Log("Found grip with no rigidbody!");
#endif

                        if (grip.TryCast<WorldGrip>() != null)
                        {
                            group = GrabGroup.WORLD_GRIP;
                            serializedGrab = new SerializedWorldGrab(smallId, new SerializedTransform(grip.transform));
                            validGrip = true;
                        }
                        else
                        {
                            group = GrabGroup.STATIC;
                            serializedGrab = new SerializedStaticGrab(grip.transform.GetPath());
                            validGrip = true;
                        }
                    }
                    // Check for prop grips
                    else if (grip.HasRigidbody  && !grip.GetComponentInParent<RigManager>())
                    {
                        group = GrabGroup.PROP;
                        GetGripInfo(grip, out var root, out var otherGrips);

                        // Do we already have a synced object?
                        if (PropSyncable.Cache.TryGetValue(root, out var syncable))
                        {
                            serializedGrab = new SerializedPropGrab("_", syncable.GetIndex(grip).Value, syncable.GetId(), true);
                            validGrip = true;
                        }
                        // Create a new one
                        else if (!NetworkInfo.IsServer)
                        {
                            syncable = new PropSyncable(root, otherGrips);

                            ushort queuedId = SyncManager.QueueSyncable(syncable);

                            using (var writer = FusionWriter.Create())
                            {
                                using (var data = SyncableIDRequestData.Create(smallId, queuedId))
                                {
                                    writer.Write(data);

                                    using (var message = FusionMessage.Create(NativeMessageTag.SyncableIDRequest, writer))
                                    {
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
                        else if (NetworkInfo.IsServer)
                        {
                            syncable = new PropSyncable(root, otherGrips);
                            SyncManager.RegisterSyncable(syncable, SyncManager.AllocateSyncID());
                            serializedGrab = new SerializedPropGrab(grip.Host.Rb.transform.GetPath(), syncable.GetIndex(grip).Value, syncable.Id, true);

                            validGrip = true;
                        }
                    }
                    // Nothing left
                    else
                    {
#if DEBUG
                        FusionLogger.Log("Found no valid grip for syncing!");
#endif
                    }
                }

                // Now, send the message
                if (validGrip)
                {
                    using (var writer = FusionWriter.Create())
                    {
                        using (var data = PlayerRepGrabData.Create(smallId, handedness, group, serializedGrab))
                        {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepGrab, writer))
                            {
                                MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
        }

        public static void SendObjectDetach(Handedness handedness)
        {
            if (NetworkInfo.HasServer)
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = PlayerRepReleaseData.Create(PlayerIdManager.LocalSmallId, handedness))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepRelease, writer))
                        {
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

    }
}
