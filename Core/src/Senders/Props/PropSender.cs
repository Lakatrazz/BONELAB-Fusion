using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Syncables;

using MelonLoader;

using SLZ.Marrow.Warehouse;
using SLZ.Rig;

namespace LabFusion.Senders
{
    public static class PropSender {
        /// <summary>
        /// Sends an ownership transfer request for a syncable.
        /// </summary>
        /// <param name="syncable"></param>
        public static void SendOwnershipTransfer(ISyncable syncable)
        {
            ushort id = syncable.GetId();

            var owner = PlayerIdManager.LocalSmallId;

            // Broadcast response
            if (NetworkInfo.IsServer)
            {
                syncable.SetOwner(owner);

                using (var writer = FusionWriter.Create(SyncableOwnershipResponseData.Size))
                {
                    using (var response = SyncableOwnershipResponseData.Create(owner, id))
                    {
                        writer.Write(response);

                        using (var message = FusionMessage.Create(NativeMessageTag.SyncableOwnershipResponse, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
            // Send request to server
            else
            {
                using (var writer = FusionWriter.Create(SyncableOwnershipRequestData.Size))
                {
                    using (var response = SyncableOwnershipRequestData.Create(owner, id))
                    {
                        writer.Write(response);

                        using (var message = FusionMessage.Create(NativeMessageTag.SyncableOwnershipRequest, writer))
                        {
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends an ownership transfer request for a syncable.
        /// </summary>
        /// <param name="syncableId"></param>
        public static void SendOwnershipTransfer(ushort syncableId)
        {
            if (!SyncManager.TryGetSyncable(syncableId, out var syncable))
                return;

            SendOwnershipTransfer(syncable);
        }

        /// <summary>
        /// Sends a message notifying others the syncable has fallen asleep/is not syncing.
        /// </summary>
        /// <param name="syncable"></param>
        public static void SendSleep(PropSyncable syncable) {
            if (NetworkInfo.HasServer) {
                using (var writer = FusionWriter.Create(PropSyncableSleepData.Size))
                {
                    using (var data = PropSyncableSleepData.Create(syncable.GetOwner().Value, syncable.GetId()))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PropSyncableSleep, writer))
                        {
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends a catchup sync message for a scene object.
        /// </summary>
        /// <param name="syncable"></param>
        public static void SendCatchupCreation(PropSyncable syncable, ulong userId) {
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create(PropSyncableCreateData.Size))
                {
                    using (var data = PropSyncableCreateData.Create(syncable.GetOwner().Value, syncable.GameObject, syncable.Id))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PropSyncableCreate, writer))
                        {
                            MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a syncable for a prop and sends it to the server.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="onFinished"></param>
        public static void SendPropCreation(GameObject prop, Action<PropSyncable> onFinished = null, bool waitUntilEnabled = false) {
            var root = prop.GetSyncRoot();

            if (PropSyncable.Cache.TryGet(root, out var syncable) || PropSyncable.HostCache.TryGet(root, out syncable)) {
                onFinished?.Invoke(syncable);
            }
            else {
                MelonCoroutines.Start(Internal_InitializePropSyncable(root, onFinished, waitUntilEnabled));
            }
        }

        private static IEnumerator Internal_InitializePropSyncable(GameObject root, Action<PropSyncable> onFinished, bool waitUntilEnabled) {
            // Wait for level to finish loading
            while (FusionSceneManager.IsDelayedLoading())
                yield return null;
            
            // Wait for the gameObject to be enabled, if you are expecting it to be disabled
            if (waitUntilEnabled) {
                while (!root.activeInHierarchy)
                    yield return null;
            }

            // Double check the root doesn't already have a syncable, incase it was synced while we were waiting
            if (PropSyncable.Cache.ContainsSource(root))
                yield break;

            // Create syncable
            var newSyncable = new PropSyncable(null, root);

            // We aren't a server. Request an id.
            if (!NetworkInfo.IsServer) {
                ushort queuedId = SyncManager.QueueSyncable(newSyncable);
                SyncManager.RequestSyncableID(queuedId);

                while (newSyncable.IsQueued())
                    yield return null;

                yield return null;

                if (newSyncable.IsDestroyed())
                    yield break;

                using (var writer = FusionWriter.Create(PropSyncableCreateData.Size))
                {
                    using (var data = PropSyncableCreateData.Create(PlayerIdManager.LocalSmallId, newSyncable.GameObject, newSyncable.Id))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PropSyncableCreate, writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }

                yield return null;
            }
            // We are a server, we can just register it
            else if (NetworkInfo.IsServer) {
                SyncManager.RegisterSyncable(newSyncable, SyncManager.AllocateSyncID());

                using (var writer = FusionWriter.Create(PropSyncableCreateData.Size))
                {
                    using (var data = PropSyncableCreateData.Create(PlayerIdManager.LocalSmallId, newSyncable.GameObject, newSyncable.Id))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PropSyncableCreate, writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }

                yield return null;
            }

            if (newSyncable.IsDestroyed())
                yield break;

            newSyncable.SetOwner(PlayerIdManager.LocalSmallId);
            onFinished?.Invoke(newSyncable);
        }
    }
}
