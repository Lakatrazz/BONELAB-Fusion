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
    public class PointItemTriggerData : IFusionSerializable, IDisposable {
        public byte smallId;
        public string barcode;

        public void Serialize(FusionWriter writer) {
            writer.Write(smallId);
            writer.Write(barcode);
        }
        
        public void Deserialize(FusionReader reader) {
            smallId = reader.ReadByte();
            barcode = reader.ReadString();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PointItemTriggerData Create(byte smallId, string barcode) {
            return new PointItemTriggerData() {
                smallId = smallId,
                barcode = barcode,
            };
        }
    }

    public class PointItemTriggerMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PointItemTrigger;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PointItemTriggerData>()) {
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
                        PointItemManager.Internal_OnTriggerItem(id, data.barcode);
                    }
                }
            }
        }
    }
}
