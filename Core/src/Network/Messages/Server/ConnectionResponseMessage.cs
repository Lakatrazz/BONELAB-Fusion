using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class ConnectionResponseData : IFusionSerializable, IDisposable {
        public PlayerId playerId = null;

        public void Serialize(FusionWriter writer) {
            writer.Write(playerId);
        }
        
        public void Deserialize(FusionReader reader) {
            playerId = reader.ReadFusionSerializable<PlayerId>();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static ConnectionResponseData Create(ulong longId, byte smallId) {
            return new ConnectionResponseData() {
                playerId = new PlayerId(longId, smallId)
            };
        }
    }

    public class ConnectionResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ConnectionResponse;

        public override void HandleMessage(byte[] bytes) {
            using (FusionReader reader = FusionReader.Create(bytes)) {
                var data = reader.ReadFusionSerializable<ConnectionResponseData>();

                // Insert the id into our list
                data.playerId.Insert();

                // Check the id to see if its our own
                // If it is, just update our self reference
                if (data.playerId.LongId == PlayerId.ConstantLongId) {
                    PlayerId.UpdateSelfId();

#if DEBUG    
                    FusionLogger.Log($"Assigned our local smallId to {data.playerId.SmallId}");
#endif
                }
                // Otherwise, create a player rep
                else {
#if DEBUG    
                    FusionLogger.Log($"Client received a join message from long id {data.playerId.LongId} and small id {data.playerId.SmallId}!");
#endif

                    new PlayerRep(data.playerId);
                }
            }
        }
    }
}
