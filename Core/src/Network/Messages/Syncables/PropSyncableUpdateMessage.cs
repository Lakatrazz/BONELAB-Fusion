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
        public byte ownerId;
        public ushort syncId;
        public byte length;
        public Vector3[] serializedPositions;
        public SerializedSmallQuaternion[] serializedQuaternions;

        public SerializedSmallVector3[] serializedVelocities;
        public SerializedSmallVector3[] serializedAngularVelocities;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(ownerId);
            writer.Write(syncId);
            writer.Write(length);

            for (var i = 0; i < serializedPositions.Length; i++) {
                var position = serializedPositions[i];
                writer.Write(position);
            }

            foreach (var rotation in serializedQuaternions)
                writer.Write(rotation);

            foreach (var velocity in serializedVelocities) {
                writer.Write(velocity);
            }

            foreach (var angularVelocity in serializedAngularVelocities) {
                writer.Write(angularVelocity);
            }
        }

        public void Deserialize(FusionReader reader)
        {
            ownerId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            length = reader.ReadByte();

            serializedPositions = new Vector3[length];
            serializedQuaternions = new SerializedSmallQuaternion[length];
            serializedVelocities = new SerializedSmallVector3[length];
            serializedAngularVelocities = new SerializedSmallVector3[length];

            for (var i = 0; i < length; i++) {
                serializedPositions[i] = reader.ReadVector3();
            }

            for (var i = 0; i < length; i++) {
                serializedQuaternions[i] = reader.ReadFusionSerializable<SerializedSmallQuaternion>();
            }

            for (var i = 0; i < length; i++) {
                serializedVelocities[i] = reader.ReadFusionSerializable<SerializedSmallVector3>();
            }

            for (var i = 0; i < length; i++) {
                serializedAngularVelocities[i] = reader.ReadFusionSerializable<SerializedSmallVector3>();
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
                serializedVelocities = new SerializedSmallVector3[length],
                serializedAngularVelocities = new SerializedSmallVector3[length],
            };

            for (var i = 0; i < length; i++) {
                var host = hosts[i];

                data.serializedPositions[i] = host.position;
                data.serializedQuaternions[i] = SerializedSmallQuaternion.Compress(host.rotation);

                var rb = rigidbodies[i];
                if (rb != null) {
                    data.serializedVelocities[i] = SerializedSmallVector3.Compress(rb.velocity * Time.timeScale);
                    data.serializedAngularVelocities[i] = SerializedSmallVector3.Compress(rb.angularVelocity * Time.timeScale);
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
                    if (syncable != null && syncable.IsRegistered() && syncable.Owner.HasValue && syncable.Owner.Value == data.ownerId) {
                        syncable.TimeOfMessage = Time.timeSinceLevelLoad;
                        
                        for (var i = 0; i < data.length; i++) {
                            syncable.DesiredPositions[i] = data.serializedPositions[i];
                            syncable.DesiredRotations[i] = data.serializedQuaternions[i].Expand();
                            syncable.DesiredVelocities[i] = data.serializedVelocities[i].Expand();
                            syncable.DesiredAngularVelocities[i] = data.serializedAngularVelocities[i].Expand();
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
