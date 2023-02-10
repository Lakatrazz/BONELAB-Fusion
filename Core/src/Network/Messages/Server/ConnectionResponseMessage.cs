using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class ConnectionResponseData : IFusionSerializable, IDisposable {
        public PlayerId playerId = null;
        public string avatarBarcode = null;
        public SerializedAvatarStats avatarStats = null;
        public bool isInitialJoin = false;

        public void Serialize(FusionWriter writer) {
            writer.Write(playerId);
            writer.Write(avatarBarcode);
            writer.Write(avatarStats);
            writer.Write(isInitialJoin);
        }
        
        public void Deserialize(FusionReader reader) {
            playerId = reader.ReadFusionSerializable<PlayerId>();
            avatarBarcode = reader.ReadString();
            avatarStats = reader.ReadFusionSerializable<SerializedAvatarStats>();
            isInitialJoin = reader.ReadBoolean();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static ConnectionResponseData Create(PlayerId id, string avatarBarcode, SerializedAvatarStats stats, bool isInitialJoin) {
            return new ConnectionResponseData() {
                playerId = id,
                avatarBarcode = avatarBarcode,
                avatarStats = stats,
                isInitialJoin = isInitialJoin,
            };
        }
    }

    public class ConnectionResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ConnectionResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (FusionReader reader = FusionReader.Create(bytes)) {
                // This should only ever be handled client side!
                if (isServerHandled)
                    throw new ExpectedClientException();

                var data = reader.ReadFusionSerializable<ConnectionResponseData>();

                // Insert the id into our list
                data.playerId.Insert();

                // Check the id to see if its our own
                // If it is, just update our self reference
                if (data.playerId.LongId == PlayerIdManager.LocalLongId) {
                    PlayerIdManager.ApplyLocalId();

                    InternalServerHelpers.OnJoinServer();
                }
                // Otherwise, create a player rep
                else {
                    InternalServerHelpers.OnUserJoin(data.playerId, data.isInitialJoin);
                    var rep = new PlayerRep(data.playerId);
                    rep.SwapAvatar(data.avatarStats, data.avatarBarcode);
                }

                // Update our vitals to everyone
                if (RigData.RigReferences.RigManager)
                    RigData.OnSendVitals();
            }
        }
    }
}
