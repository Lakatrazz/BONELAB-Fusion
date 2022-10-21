using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Syncables;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Network
{
    public class PropSyncableUpdateData : IFusionSerializable, IDisposable
    {
        public byte ownerId;
        public ushort syncId;
        public SerializedTransform[] serializedTransforms;
        public Vector3[] velocities;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(ownerId);
            writer.Write(syncId);
            writer.Write((byte)serializedTransforms.Length);

            foreach (var transform in serializedTransforms)
                writer.Write(transform);

            foreach (var vel in velocities)
                writer.Write(vel);
        }

        public void Deserialize(FusionReader reader)
        {
            ownerId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            byte transformCount = reader.ReadByte();
            serializedTransforms = new SerializedTransform[transformCount];
            velocities = new Vector3[transformCount];

            for (var i = 0; i < transformCount; i++)
                serializedTransforms[i] = reader.ReadFusionSerializable<SerializedTransform>();

            for (var i = 0; i < transformCount; i++)
                velocities[i] = reader.ReadVector3();
        }

        public PropSyncable GetPropSyncable() {
            if (SyncManager.TryGetSyncable(syncId, out var syncable) && syncable is PropSyncable propSyncable)
                return propSyncable;

            return null;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PropSyncableUpdateData Create(byte ownerId, ushort syncId, GameObject[] hosts, Rigidbody[] rigidbodies)
        {
            var data = new PropSyncableUpdateData {
                ownerId = ownerId,
                syncId = syncId,
                serializedTransforms = new SerializedTransform[rigidbodies.Length],
                velocities = new Vector3[rigidbodies.Length]
            };

            for (var i = 0; i < rigidbodies.Length; i++) {
                var rb = rigidbodies[i];
                var host = hosts[i];

                if (host != null) {
                    data.serializedTransforms[i] = new SerializedTransform(host.transform);
                }
                else
                    data.serializedTransforms[i] = new SerializedTransform(Vector3.zero, Quaternion.identity);

                if (rb != null) {
                    data.velocities[i] = rb.velocity;
                }
                else
                    data.velocities[i] = Vector3.zero;
            }

            return data;
        }
    }

    public class PropSyncableUpdateMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PropSyncableUpdate;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PropSyncableUpdateData>()) {
                    // Find the prop syncable and update its info
                    var syncable = data.GetPropSyncable();
                    if (syncable != null && syncable.Owner.HasValue && syncable.Owner.Value == data.ownerId) {
                        syncable.TimeSinceUpdateReceived = Time.realtimeSinceStartup;

                        for (var i = 0; i < data.serializedTransforms.Length; i++) {
                            syncable.DesiredPositions[i] = data.serializedTransforms[i].position;
                            syncable.DesiredRotations[i] = data.serializedTransforms[i].rotation.Expand();
                            syncable.DesiredVelocities[i] = data.velocities[i];
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
