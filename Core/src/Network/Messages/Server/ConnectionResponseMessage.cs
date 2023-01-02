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
        public string avatarBarcode = null;
        public SerializedAvatarStats avatarStats = null;

        public void Serialize(FusionWriter writer) {
            writer.Write(playerId);
            writer.Write(avatarBarcode);
            writer.Write(avatarStats);
        }
        
        public void Deserialize(FusionReader reader) {
            playerId = reader.ReadFusionSerializable<PlayerId>();
            avatarBarcode = reader.ReadString();
            avatarStats = reader.ReadFusionSerializable<SerializedAvatarStats>();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static ConnectionResponseData Create(ulong longId, byte smallId, string username, string avatarBarcode, SerializedAvatarStats stats) {
            return new ConnectionResponseData() {
                playerId = new PlayerId(longId, smallId, username),
                avatarBarcode = avatarBarcode,
                avatarStats = stats,
            };
        }
    }

    public class ConnectionResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ConnectionResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (FusionReader reader = FusionReader.Create(bytes)) {
                var data = reader.ReadFusionSerializable<ConnectionResponseData>();

                // Insert the id into our list
                data.playerId.Insert();

                // Check the id to see if its our own
                // If it is, just update our self reference
                if (data.playerId.LongId == PlayerIdManager.LocalLongId) {
                    PlayerIdManager.ApplyLocalId();

                    if (RigData.RigReferences.RigManager)
                        RigData.OnRigRescale();

                    InternalServerHelpers.OnJoinServer();
#if DEBUG    
                    FusionLogger.Log($"Assigned our local smallId to {data.playerId.SmallId}, real id was {PlayerIdManager.LocalSmallId}");
#endif
                }
                // Otherwise, create a player rep
                else {
#if DEBUG    
                    FusionLogger.Log($"Client received a join message from long id {data.playerId.LongId} and small id {data.playerId.SmallId}!");
#endif
                    InternalServerHelpers.OnUserJoin(data.playerId);
                    var rep = new PlayerRep(data.playerId);
                    rep.SwapAvatar(data.avatarStats, data.avatarBarcode);
                }
            }
        }
    }
}
