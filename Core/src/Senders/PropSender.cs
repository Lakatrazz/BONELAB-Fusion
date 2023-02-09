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
using SLZ.Marrow.Warehouse;
using SLZ.Rig;

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
            // Make sure this isn't a rig
            if (prop.GetComponentInParent<RigManager>())
                return;
            
            var root = prop.GetSyncRoot();

            if (PropSyncable.Cache.TryGet(root, out var syncable) || PropSyncable.HostCache.TryGet(root, out syncable)) {
                onFinished?.Invoke(syncable);
            }
            else {
                MelonCoroutines.Start(Internal_InitializePropSyncable(root, onFinished, waitUntilEnabled));
            }
        }

        /// <summary>
        /// Sends the OnPlaceEvent for a SpawnableCratePlacer.
        /// </summary>
        /// <param name="placer"></param>
        /// <param name="go"></param>
        public static void SendCratePlacerEvent(SpawnableCratePlacer placer, GameObject go) {
            if (NetworkInfo.IsServer) {
                MelonCoroutines.Start(Internal_WaitForCratePlacer(placer, go));
            }
        }
        
        private static IEnumerator Internal_WaitForCratePlacer(SpawnableCratePlacer placer, GameObject go) {
            while (LevelWarehouseUtilities.IsLoading())
                yield return null;

            for (var i = 0; i < 5; i++)
                yield return null;

            if (PropSyncable.Cache.TryGet(go, out var syncable))
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = SpawnableCratePlacerData.Create(syncable.GetId(), placer.gameObject))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.SpawnableCratePlacer, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
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

            newSyncable.SetOwner(PlayerIdManager.LocalSmallId);
            onFinished?.Invoke(newSyncable);
        }
    }
}
