using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public class DisconnectMessageData : IFusionSerializable, IDisposable {
        public ulong longId;

        public static DisconnectMessageData Create(ulong longId) {
            return new DisconnectMessageData() {
                longId = longId
            };
        }

        public void Serialize(FusionWriter writer) {
            writer.Write(longId);
        }

        public void Deserialize(FusionReader reader) {
            longId = reader.ReadUInt64();
        }

        public void Dispose() {
            var playerId = PlayerIdManager.GetPlayerId(longId);
            if (playerId != null)
                playerId.Dispose();

            GC.SuppressFinalize(this);
        }
    }

    public class DisconnectMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.Disconnect;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (!NetworkInfo.IsServer) {
                using (var reader = FusionReader.Create(bytes)) {
                    var data = reader.ReadFusionSerializable<DisconnectMessageData>();

                    // If this is our id, disconnect ourselves
                    if (data.longId == PlayerIdManager.LocalLongId) {
                        NetworkHelper.Disconnect();

#if DEBUG
                        FusionLogger.Log("The server has requested you disconnect.");
#endif
                    }
                    // Otherwise, disconnect the other person in the lobby
                    else {
                        InternalServerHelpers.OnUserLeave(data.longId);
                    }
                }
            }
        }
    }
}
