using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Syncables;

using SLZ.Props;

namespace LabFusion.Network
{
    public class ConstrainerModeData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 2 + sizeof(ushort);

        public byte smallId;
        public ushort constrainerId;
        public Constrainer.ConstraintMode mode;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(constrainerId);
            writer.Write((byte)mode);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            constrainerId = reader.ReadUInt16();
            mode = (Constrainer.ConstraintMode)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ConstrainerModeData Create(byte smallId, ushort constrainerId, Constrainer.ConstraintMode mode)
        {
            return new ConstrainerModeData()
            {
                smallId = smallId,
                constrainerId = constrainerId,
                mode = mode,
            };
        }
    }

    [Net.SkipHandleWhileLoading]
    public class ConstrainerModeMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ConstrainerMode;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            using var data = reader.ReadFusionSerializable<ConstrainerModeData>();

            // Send message to all clients if server
            if (NetworkInfo.IsServer && isServerHandled) {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            }
            else {
                // Play the SFX and change the mode
                if (SyncManager.TryGetSyncable<PropSyncable>(data.constrainerId, out var constrainer) && constrainer.TryGetExtender<ConstrainerExtender>(out var extender)) {
                    var comp = extender.Component;
                    comp.PlaySFX(comp.modeSFX, comp.firePoint.position);

                    comp.mode = data.mode;
                }
            }
        }
    }
}
