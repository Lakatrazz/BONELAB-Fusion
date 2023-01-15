using LabFusion.Syncables;

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using LabFusion.Grabbables;

using MelonLoader;
using LabFusion.Network;
using LabFusion.Representation;
using SLZ.Interaction;
using LabFusion.Utilities;

namespace LabFusion.Senders
{
    public static class PropSender {
        /// <summary>
        /// Sends a catchup sync message for a scene object.
        /// </summary>
        /// <param name="syncable"></param>
        public static void SendCatchupCreation(PropSyncable syncable, ulong userId) {
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = PropSyncableCreateData.Create(PlayerIdManager.LocalSmallId, syncable.GameObject, syncable.Id))
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
            while (LevelWarehouseUtilities.IsDelayedLoading())
                yield return null;
            
            // Wait for the gameObject to be enabled, if you are expecting it to be disabled
            if (waitUntilEnabled) {
                while (!root.activeInHierarchy)
                    yield return null;
            }

            // Create syncable
            var newSyncable = new PropSyncable(null, root);
            newSyncable.SetOwner(PlayerIdManager.LocalSmallId);
            
            // We aren't a server. Request an id.
            if (!NetworkInfo.IsServer) {
                ushort queuedId = SyncManager.QueueSyncable(newSyncable);
                SyncManager.RequestSyncableID(queuedId);

                while (newSyncable.IsQueued())
                    yield return null;

                yield return null;

                using (var writer = FusionWriter.Create())
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

                using (var writer = FusionWriter.Create())
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

            onFinished?.Invoke(newSyncable);
        }
    }
}
