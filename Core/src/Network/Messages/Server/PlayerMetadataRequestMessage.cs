using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;

using System;
using LabFusion.Senders;
using LabFusion.Exceptions;

namespace LabFusion.Network
{
    public class PlayerMetadataRequestData : IFusionSerializable, IDisposable {
        public byte smallId;
        public string key;
        public string value;

        public void Serialize(FusionWriter writer) {
            writer.Write(smallId);
            writer.Write(key);
            writer.Write(value);
        }
        
        public void Deserialize(FusionReader reader) {
            smallId = reader.ReadByte();
            key = reader.ReadString();
            value = reader.ReadString();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PlayerMetadataRequestData Create(byte smallId, string key, string value) {
            return new PlayerMetadataRequestData() {
                smallId = smallId,
                key = key,
                value = value,
            };
        }
    }

    public class PlayerMetadataRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerMetadataRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            // This should only ever be handled by the server
            if (NetworkInfo.IsServer && isServerHandled) {
                using (FusionReader reader = FusionReader.Create(bytes)) {
                    using (var data = reader.ReadFusionSerializable<PlayerMetadataRequestData>()) {

                        // Send the response to all clients
                        PlayerSender.SendPlayerMetadataResponse(data.smallId, data.key, data.value);
                    }
                }
            }
            else
                throw new ExpectedServerException();
        }
    }
}
