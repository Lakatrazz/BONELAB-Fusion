using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using System;
using static Valve.VR.IVRIOBuffer;

namespace LabFusion.Network
{
    public class ConnectionRequestData : IFusionSerializable, IDisposable {
        public ulong longId;

        public void Serialize(FusionWriter writer) {
            writer.Write(longId);
        }
        
        public void Deserialize(FusionReader reader) {
            longId = reader.ReadUInt64();
        }

        public void Dispose() {}

        public static ConnectionRequestData Create(ulong longId) {
            return new ConnectionRequestData() {
                longId = longId
            };
        }
    }

    public class ConnectionRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ConnectionRequest;

        public override void HandleMessage(byte[] bytes) {
            if (FusionMod.CurrentNetworkLayer.IsServer) {
                using (FusionReader reader = FusionReader.Create(bytes)) {
                    var data = reader.ReadFusionSerializable<ConnectionRequestData>();
                    var newSmallId = PlayerId.GetUnusedPlayerId();

                    if (PlayerId.GetPlayerId(data.longId) == null && newSmallId.HasValue) {

#if DEBUG
                        FusionLogger.Log($"Server received user with long id {data.longId}. Assigned small id {newSmallId}");
#endif

                        // First we send the new player to all existing players (and the new player so they know they exist)
                        using (FusionWriter writer = FusionWriter.Create()) {
                            using (var response = ConnectionResponseData.Create(data.longId, newSmallId.Value)) {
                                writer.Write(response);

                                using (var message = FusionMessage.Create(NativeMessageTag.ConnectionResponse, writer)) {
                                    FusionMod.CurrentNetworkLayer.BroadcastMessage(NetworkChannel.Reliable, message);
                                }
                            }
                        }

                        // Now we send all of our other players to the new player
                        foreach (var id in PlayerId.PlayerIds) {
                            using (FusionWriter writer = FusionWriter.Create()) {
                                using (var response = ConnectionResponseData.Create(id.LongId, id.SmallId)) {
                                    writer.Write(response);

                                    using (var message = FusionMessage.Create(NativeMessageTag.ConnectionResponse, writer)) {
                                        FusionMod.CurrentNetworkLayer.SendServerMessage(data.longId, NetworkChannel.Reliable, message);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
