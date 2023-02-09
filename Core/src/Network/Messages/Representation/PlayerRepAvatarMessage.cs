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
        public const int DefaultSize = sizeof(byte) + SerializedAvatarStats.Size; 

        public byte smallId;
        public SerializedAvatarStats stats;
        public string barcode;

        public void Serialize(FusionWriter writer) {
            writer.Write(smallId);
            writer.Write(stats);
            writer.Write(barcode);
        }

        public void Deserialize(FusionReader reader) { 
            smallId = reader.ReadByte();
            stats = reader.ReadFusionSerializable<SerializedAvatarStats>();
            barcode = reader.ReadString();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepAvatarData Create(byte smallId, SerializedAvatarStats stats, string barcode) {
            return new PlayerRepAvatarData()
            {
                smallId = smallId,
                stats = stats,
                barcode = barcode
            };
        }
    }

    public class PlayerRepAvatarMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.PlayerRepAvatar;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PlayerRepAvatarData>()) {
                    // Swap the avatar for the rep
                    if (PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep)) {
                        rep.SwapAvatar(data.stats, data.barcode);
                    }

                    // Bounce the message back
                    if (NetworkInfo.IsServer) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
