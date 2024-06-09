using UnityEngine;

using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Syncables;

using LabFusion.MonoBehaviours;

namespace LabFusion.Senders
{
    public static class PropSender
    {
        /// <summary>
        /// Sends an ownership transfer request for a syncable.
        /// </summary>
        /// <param name="syncable"></param>
        public static void SendOwnershipTransfer<TSyncable>(TSyncable syncable) where TSyncable : ISyncable
        {
            ushort id = syncable.GetId();

            var owner = PlayerIdManager.LocalSmallId;

            // Broadcast response
            if (NetworkInfo.IsServer)
            {
                syncable.SetOwner(owner);

                using var writer = FusionWriter.Create(SyncableOwnershipResponseData.Size);
                var response = SyncableOwnershipResponseData.Create(owner, id);
                writer.Write(response);

                using var message = FusionMessage.Create(NativeMessageTag.SyncableOwnershipResponse, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
            // Send request to server
            else
            {
                using var writer = FusionWriter.Create(SyncableOwnershipRequestData.Size);
                var response = SyncableOwnershipRequestData.Create(owner, id);
                writer.Write(response);

                using var message = FusionMessage.Create(NativeMessageTag.SyncableOwnershipRequest, writer);
                MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
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
        public static void SendSleep(PropSyncable syncable)
        {
            if (NetworkInfo.HasServer)
            {
                using var writer = FusionWriter.Create(PropSyncableSleepData.Size);
                var data = PropSyncableSleepData.Create(syncable.GetOwner().Value, syncable.GetId());
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PropSyncableSleep, writer);
                MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
            }
        }

        /// <summary>
        /// Sends a catchup sync message for a scene object.
        /// </summary>
        /// <param name="syncable"></param>
        public static void SendCatchupCreation(PropSyncable syncable, ulong userId)
        {
            if (NetworkInfo.IsServer)
            {
                using var writer = FusionWriter.Create(PropSyncableCreateData.Size);
                var data = PropSyncableCreateData.Create(syncable.GetOwner().Value, syncable.GameObject.GetFullPath(), syncable.Id);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PropSyncableCreate, writer);
                MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
            }
        }

        private struct PropCreationInfo
        {
            public GameObject root;
            public Action<PropSyncable> onFinished;
            public bool waitUntilEnabled;
        }

        /// <summary>
        /// Creates a syncable for a prop and sends it to the server.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="onFinished"></param>
        public static void SendPropCreation(GameObject prop, Action<PropSyncable> onFinished = null, bool waitUntilEnabled = false)
        {
            var root = prop.GetSyncRoot();

            if (PropSyncable.Cache.TryGet(root, out var syncable) || PropSyncable.HostCache.TryGet(root, out syncable))
            {
                onFinished?.Invoke(syncable);
            }
            else
            {
                var info = new PropCreationInfo()
                {
                    root = root,
                    onFinished = onFinished,
                    waitUntilEnabled = waitUntilEnabled,
                };
                FusionSceneManager.HookOnDelayedLevelLoad(() => { Internal_InitializePropSyncable(info); });
            }
        }

        private static void Internal_InitializePropSyncable(PropCreationInfo info)
        {
            PropSyncable newSyncable = null;

            // Wait until the object is enabled?
            if (info.waitUntilEnabled)
            {
                var notify = info.root.AddComponent<NotifyOnEnable>();
                notify.Hook(OnStart);
            }
            else
            {
                OnStart();
            }

            void OnStart()
            {
                // Double check the root doesn't already have a syncable, incase it was synced while we were waiting
                if (PropSyncable.Cache.ContainsSource(info.root))
                    return;

                // Create syncable
                newSyncable = new PropSyncable(null, info.root);

                // We aren't a server. Request an id.
                if (!NetworkInfo.IsServer)
                {
                    ushort queuedId = SyncManager.QueueSyncable(newSyncable);
                    SyncManager.RequestSyncableID(queuedId);

                    newSyncable.HookOnRegistered(() =>
                    {
                        _ = newSyncable.GameObject.GetFullPathAsync(OnPathReceived);
                    });
                }
                // We are a server, we can just register it
                else
                {
                    SyncManager.RegisterSyncable(newSyncable, SyncManager.AllocateSyncID());

                    newSyncable.HookOnRegistered(() =>
                    {
                        _ = newSyncable.GameObject.GetFullPathAsync(OnPathReceived);
                    });
                }
            }

            void OnPathReceived(string result)
            {
                if (newSyncable.IsDestroyed())
                    return;

                using var writer = FusionWriter.Create(PropSyncableCreateData.Size);
                var data = PropSyncableCreateData.Create(PlayerIdManager.LocalSmallId, result, newSyncable.Id);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PropSyncableCreate, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);

                OnFinish();
            }

            void OnFinish()
            {
                newSyncable.SetOwner(PlayerIdManager.LocalSmallId);
                info.onFinished?.Invoke(newSyncable);
            }
        }
    }
}
