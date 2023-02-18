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
    public class PointItemEquipStateData : IFusionSerializable, IDisposable {
        public byte smallId;
        public string barcode;
        public bool isEquipped;

        public void Serialize(FusionWriter writer) {
            writer.Write(smallId);
            writer.Write(barcode);
            writer.Write(isEquipped);
        }
        
        public void Deserialize(FusionReader reader) {
            smallId = reader.ReadByte();
            barcode = reader.ReadString();
            isEquipped = reader.ReadBoolean();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PointItemEquipStateData Create(byte smallId, string barcode, bool isEquipped) {
            return new PointItemEquipStateData() {
                smallId = smallId,
                barcode = barcode,
                isEquipped = isEquipped,
            };
        }
    }

    public class PointItemEquipStateMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PointItemEquipState;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PointItemEquipStateData>()) {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled)
                    {
                        using (var message = FusionMessage.Create(Tag.Value, bytes))
                        {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else if (PointItemManager.TryGetPointItem(data.barcode, out var item)) {
                        var id = PlayerIdManager.GetPlayerId(data.smallId);

                        id.Internal_ForceSetEquipped(data.barcode, data.isEquipped);
                    }
                }
            }
        }
    }
}
