using UnityEngine;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.MonoBehaviours;

using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Senders
{
    public static class PropSender
    {
        /// <summary>
        /// Sends a catchup sync message for a scene object.
        /// </summary>
        /// <param name="prop"></param>
        public static void SendCatchupCreation(NetworkProp prop, ulong userId)
        {
            if (NetworkInfo.IsServer)
            {
                using var writer = FusionWriter.Create(PropSyncableCreateData.Size);
                var networkEntity = prop.NetworkEntity;

                var data = PropSyncableCreateData.Create(networkEntity.OwnerId, prop.MarrowEntity.gameObject.GetFullPath(), networkEntity.Id);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PropSyncableCreate, writer);
                MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
            }
        }

        private struct PropCreationInfo
        {
            public MarrowEntity marrowEntity;
            public Action<NetworkEntity> onFinished;
            public bool waitUntilEnabled;
        }

        /// <summary>
        /// Creates a syncable for a prop and sends it to the server.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="onFinished"></param>
        public static void SendPropCreation(MarrowEntity marrowEntity, Action<NetworkEntity> onFinished = null, bool waitUntilEnabled = false)
        {
            if (IMarrowEntityExtender.Cache.TryGet(marrowEntity, out var entity))
            {
                onFinished?.Invoke(entity);
            }
            else
            {
                var info = new PropCreationInfo()
                {
                    marrowEntity = marrowEntity,
                    onFinished = onFinished,
                    waitUntilEnabled = waitUntilEnabled,
                };
                FusionSceneManager.HookOnDelayedLevelLoad(() => { Internal_InitializePropSyncable(info); });
            }
        }

        private static void Internal_InitializePropSyncable(PropCreationInfo info)
        {
            NetworkEntity newEntity = null;
            NetworkProp newProp = null;

            // Wait until the object is enabled?
            if (info.waitUntilEnabled)
            {
                var notify = info.marrowEntity.gameObject.AddComponent<NotifyOnEnable>();
                notify.Hook(OnStart);
            }
            else
            {
                OnStart();
            }

            void OnStart()
            {
                // Double check the marrow entity doesn't already have a network entity, incase it was synced while we were waiting
                if (IMarrowEntityExtender.Cache.ContainsSource(info.marrowEntity))
                {
                    return;
                }

                // Create entity
                newEntity = new();
                newProp = new(newEntity, info.marrowEntity);

                ushort queuedId = NetworkEntityManager.IdManager.QueueEntity(newEntity);
                NetworkEntityManager.RequestUnqueue(queuedId);

                newEntity.HookOnRegistered((entity) =>
                {
                    _ = newProp.MarrowEntity.gameObject.GetFullPathAsync(OnPathReceived);
                });
            }

            void OnPathReceived(string result)
            {
                if (newEntity.IsDestroyed)
                    return;

                using var writer = FusionWriter.Create(PropSyncableCreateData.Size);
                var data = PropSyncableCreateData.Create(PlayerIdManager.LocalSmallId, result, newEntity.Id);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PropSyncableCreate, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);

                OnFinish();
            }

            void OnFinish()
            {
                newEntity.SetOwner(PlayerIdManager.LocalId);
                info.onFinished?.Invoke(newEntity);
            }
        }
    }
}
