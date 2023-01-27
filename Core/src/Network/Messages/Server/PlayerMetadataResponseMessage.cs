using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Representation;

namespace LabFusion.Network
{
    public class PlayerMetadataResponseData : IFusionSerializable, IDisposable {
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

        public static PlayerMetadataResponseData Create(byte smallId, string key, string value) {
            return new PlayerMetadataResponseData() {
                smallId = smallId,
                key = key,
                value = value,
            };
        }
    }

    public class PlayerMetadataResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerMetadataResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (NetworkInfo.IsClient || !isServerHandled)
            {
                using (var reader = FusionReader.Create(bytes))
                {
                    using (var data = reader.ReadFusionSerializable<PlayerMetadataResponseData>())
                    {
                        var playerId = PlayerIdManager.GetPlayerId(data.smallId);

                        if (playerId != null) {
                            playerId.Internal_ForceSetMetadata(data.key, data.value);
                        }
                    }
                }
            }
            else
                throw new ExpectedClientException();
        }
    }
}
