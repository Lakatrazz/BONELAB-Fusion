using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Syncables;
using LabFusion.Patching;

using SLZ;
using SLZ.Interaction;
using SLZ.Props.Weapons;

using UnityEngine;

using SLZ.Props;

namespace LabFusion.Network
{
    public class ConstraintDeleteData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(ushort);

        public byte smallId;
        public ushort constraintId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(constraintId);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            constraintId = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ConstraintDeleteData Create(byte smallId, ushort constraintId)
        {
            return new ConstraintDeleteData()
            {
                smallId = smallId,
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
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<ConstraintDeleteData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        if (SyncManager.TryGetSyncable(data.constraintId, out var syncable) && syncable is ConstraintSyncable constraint) {
                            ConstraintTrackerPatches.IgnorePatches = true;
                            constraint.Tracker.DeleteConstraint();
                            ConstraintTrackerPatches.IgnorePatches = false;

                            SyncManager.RemoveSyncable(constraint);
                        }
                    }
                }
            }
        }
    }
}
