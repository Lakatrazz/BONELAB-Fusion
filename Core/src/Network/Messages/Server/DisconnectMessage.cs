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
            longId = reader.ReadUInt16();
        }

        public void Dispose() {
            var playerId = PlayerId.GetPlayerId(longId);
            if (playerId != null)
                playerId.Dispose();
        }
    }

    public class DisconnectMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.Disconnect;

        public override void HandleMessage(byte[] bytes) {
            if (!FusionMod.CurrentNetworkLayer.IsServer) {
                using (var reader = FusionReader.Create(bytes)) {
                    var data = reader.ReadFusionSerializable<DisconnectMessageData>();

                    // If this is our id, disconnect ourselves
                    if (data.longId == PlayerId.SelfId.LongId) {
                        FusionMod.CurrentNetworkLayer.Disconnect();

#if DEBUG
                        FusionLogger.Log("The server has requested you disconnect.");
#endif
                    }
                    // Otherwise, disconnect the other person in the lobby
                    else {
                        NetworkUtilities.RemoveUser(data.longId);
                    }
                }
            }
        }
    }
}
