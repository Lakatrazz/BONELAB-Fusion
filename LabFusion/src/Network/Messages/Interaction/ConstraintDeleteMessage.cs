using System;
using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Syncables;

namespace LabFusion.Network
{
    public class ConstraintDeleteData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(ushort) * 2;

        public byte smallId;
        public ushort constrainerId;
        public ushort constraintId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(constrainerId);
            writer.Write(constraintId);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            constrainerId = reader.ReadUInt16();
            constraintId = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ConstraintDeleteData Create(byte smallId, ushort constrainerId, ushort constraintId)
        {
            return new ConstraintDeleteData
            {
                smallId = smallId,
                constrainerId = constrainerId,
                constraintId = constraintId,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class ConstraintDeleteMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ConstraintDelete;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            using var data = reader.ReadFusionSerializable<ConstraintDeleteData>();

            bool hasConstrainer = SyncManager.TryGetSyncable<PropSyncable>(data.constrainerId, out var constrainer);

            // Send message to all clients if server
            if (NetworkInfo.IsServer && isServerHandled)
            {
                // Make sure we have a constrainer server side (and it's being held)
                if (hasConstrainer && constrainer.IsHeld && constrainer.HasExtender<ConstrainerExtender>())
                {
                    using var message = FusionMessage.Create(Tag.Value, bytes);
                    MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                }
            }
            else
            {
                if (SyncManager.TryGetSyncable<ConstraintSyncable>(data.constraintId, out var constraint))
                {
                    ConstraintTrackerPatches.IgnorePatches = true;
                    constraint.Tracker.DeleteConstraint();
                    ConstraintTrackerPatches.IgnorePatches = false;

                    SyncManager.RemoveSyncable(constraint);

                    // Play sound
                    if (data.smallId != PlayerIdManager.LocalSmallId && hasConstrainer && constrainer.TryGetExtender<ConstrainerExtender>(out var extender))
                    {
                        extender.Component.sfx.Release();
                    }
                }
            }
        }
    }
}
