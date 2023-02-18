using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.SDK.Points;
using LabFusion.Representation;

namespace LabFusion.Network
{
    public class PointItemTriggerValueData : IFusionSerializable, IDisposable {
        public byte smallId;
        public string barcode;
        public string value;

        public void Serialize(FusionWriter writer) {
            writer.Write(smallId);
            writer.Write(barcode);
            writer.Write(value);
        }
        
        public void Deserialize(FusionReader reader) {
            smallId = reader.ReadByte();
            barcode = reader.ReadString();
            value = reader.ReadString();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PointItemTriggerValueData Create(byte smallId, string barcode, string value) {
            return new PointItemTriggerValueData() {
                smallId = smallId,
                barcode = barcode,
                value = value,
            };
        }
    }

    public class PointItemTriggerValueMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PointItemTriggerValue;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PointItemTriggerValueData>()) {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled)
                    {
                        using (var message = FusionMessage.Create(Tag.Value, bytes))
                        {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        var id = PlayerIdManager.GetPlayerId(data.smallId);
                        PointItemManager.Internal_OnTriggerItem(id, data.barcode, data.value);
                    }
                }
            }
        }
    }
}
