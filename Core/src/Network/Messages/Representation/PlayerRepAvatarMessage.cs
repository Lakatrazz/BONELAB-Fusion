using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;

namespace LabFusion.Network
{
    public class PlayerRepAvatarData : IFusionSerializable, IDisposable {
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

        public static PlayerRepAvatarData Create(byte smallId, string barcode) {
            return new PlayerRepAvatarData()
            {
                smallId = smallId,
                barcode = barcode
            };
        }
    }

    public class PlayerRepAvatarMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.PlayerRepAvatar;

        public override void HandleMessage(byte[] bytes) {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PlayerRepAvatarData>()) {
                    // Swap the avatar for the rep
                    if (PlayerRep.Representations.ContainsKey(data.smallId)) {
                        var rep = PlayerRep.Representations[data.smallId];
                        rep.SwapAvatar(data.barcode);
                    }

                    // Bounce the message back
                    if (NetworkUtilities.IsServer) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            FusionMod.CurrentNetworkLayer.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
