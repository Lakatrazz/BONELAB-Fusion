using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;

using SLZ;
using SLZ.Interaction;
using LabFusion.Extensions;

namespace LabFusion.Network
{
    public class PlayerRepForceGrabData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + SerializedPropGrab.Size;

        public byte smallId;
        public SerializedPropGrab propGrab;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(propGrab);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            propGrab = reader.ReadFusionSerializable<SerializedPropGrab>();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepForceGrabData Create(byte smallId, SerializedPropGrab propGrab)
        {
            return new PlayerRepForceGrabData()
            {
                smallId = smallId,
                propGrab = propGrab,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class PlayerRepForceGrabMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepForceGrab;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<PlayerRepForceGrabData>())
                {
                    data.propGrab.GetGrip(out var syncable);

                    if (syncable != null) {
                        syncable.SetOwner(data.smallId);
                    }

                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled)
                    {
                        using (var message = FusionMessage.Create(Tag.Value, bytes))
                        {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
