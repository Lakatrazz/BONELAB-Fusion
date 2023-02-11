using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Syncables;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LabFusion.Extensions;

namespace LabFusion.Network
{
    public class PropSyncableUpdateData : IFusionSerializable, IDisposable
    {
        public const int DefaultSize = sizeof(byte) * 2 + sizeof(ushort);
        public const int RigidbodySize = sizeof(float) * 9 + SerializedSmallQuaternion.Size;

        public byte ownerId;
        public ushort syncId;
        public byte length;
        public Vector3[] serializedPositions;
        public SerializedSmallQuaternion[] serializedQuaternions;

        public Vector3[] serializedVelocities;
        public Vector3[] serializedAngularVelocities;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(ownerId);
            writer.Write(syncId);
            writer.Write(length);

            for (var i = 0; i < length; i++) {
                writer.Write(serializedPositions[i]);
                writer.Write(serializedQuaternions[i]);
                writer.Write(serializedVelocities[i]);
                writer.Write(serializedAngularVelocities[i]);
            }
        }

        public void Deserialize(FusionReader reader)
        {
            ownerId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            length = reader.ReadByte();

            serializedPositions = new Vector3[length];
            serializedQuaternions = new SerializedSmallQuaternion[length];
            serializedVelocities = new Vector3[length];
            serializedAngularVelocities = new Vector3[length];

            for (var i = 0; i < length; i++) {
                serializedPositions[i] = reader.ReadVector3();
                serializedQuaternions[i] = reader.ReadFusionSerializable<SerializedSmallQuaternion>();
                serializedVelocities[i] = reader.ReadVector3();
                serializedAngularVelocities[i] = reader.ReadVector3();
            }
        }

        public PropSyncable GetPropSyncable() {
            if (SyncManager.TryGetSyncable(syncId, out var syncable) && syncable is PropSyncable propSyncable)
                return propSyncable;

            return null;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PropSyncableUpdateData Create(byte ownerId, PropSyncable syncable)
        {
            var syncId = syncable.GetId();
            var hosts = syncable.HostTransforms;
            var rigidbodies = syncable.Rigidbodies;

            int length = rigidbodies.Length;

            var data = new PropSyncableUpdateData {
                ownerId = ownerId,
                syncId = syncId,
                length = (byte)length,
                serializedPositions = new Vector3[length],
                serializedQuaternions = new SerializedSmallQuaternion[length],
                serializedVelocities = new Vector3[length],
                serializedAngularVelocities = new Vector3[length],
            };

            for (var i = 0; i < length; i++) {
                var host = hosts[i];

                data.serializedPositions[i] = host.position;
                data.serializedQuaternions[i] = SerializedSmallQuaternion.Compress(host.rotation);

                if (!syncable.IsRigidbodyNull(i)) {
                    var rb = rigidbodies[i];

                    data.serializedVelocities[i] = rb.velocity * Time.timeScale;
                    data.serializedAngularVelocities[i] = rb.angularVelocity * Time.timeScale;
                }
            }

            return data;
        }
    }

    [Net.SkipHandleWhileLoading]
    public class PropSyncableUpdateMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PropSyncableUpdate;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PropSyncableUpdateData>()) {
                    // Find the prop syncable and update its info
                    var syncable = data.GetPropSyncable();
                    if (syncable != null && syncable.IsRegistered() && syncable.Owner.HasValue && syncable.Owner.Value == data.ownerId && syncable.HostGameObjects.Length == data.length) {
                        syncable.RefreshMessageTime();
                        syncable.ReceivedMessage = true;
                        
                        for (var i = 0; i < data.length; i++) {
                            syncable.InitialPositions[i] = data.serializedPositions[i];
                            syncable.InitialRotations[i] = data.serializedQuaternions[i].Expand();
                            
                            syncable.DesiredPositions[i] = data.serializedPositions[i];
                            syncable.DesiredRotations[i] = data.serializedQuaternions[i].Expand();

                            syncable.DesiredVelocities[i] = data.serializedVelocities[i];
                            syncable.DesiredAngularVelocities[i] = data.serializedAngularVelocities[i];
                        }
                    }

                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.ownerId, NetworkChannel.Unreliable, message);
                        }
                    }
                }
            }
        }
    }
}
