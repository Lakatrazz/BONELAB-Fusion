using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Utilities;

using System;

namespace LabFusion.Network
{
    public class ConnectionRequestData : IFusionSerializable, IDisposable {
        public ulong longId;
        public string username;
        public string avatarBarcode;

        public void Serialize(FusionWriter writer) {
            writer.Write(longId);
            writer.Write(username);
            writer.Write(avatarBarcode);
        }
        
        public void Deserialize(FusionReader reader) {
            longId = reader.ReadUInt64();
            username = reader.ReadString();
            avatarBarcode = reader.ReadString();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static ConnectionRequestData Create(ulong longId, string username, string avatarBarcode) {
            return new ConnectionRequestData() {
                longId = longId,
                username = username,
                avatarBarcode = avatarBarcode,
            };
        }
    }

    public class ConnectionRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ConnectionRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (NetworkInfo.CurrentNetworkLayer.IsServer) {
                using (FusionReader reader = FusionReader.Create(bytes)) {
                    var data = reader.ReadFusionSerializable<ConnectionRequestData>();
                    var newSmallId = PlayerIdManager.GetUnusedPlayerId();

                    if (PlayerIdManager.GetPlayerId(data.longId) == null && newSmallId.HasValue) {

#if DEBUG
                        FusionLogger.Log($"Server received user with long id {data.longId}. Assigned small id {newSmallId}");
#endif

                        // First we send the new player to all existing players (and the new player so they know they exist)
                        using (FusionWriter writer = FusionWriter.Create()) {
                            using (var response = ConnectionResponseData.Create(data.longId, newSmallId.Value, data.username, data.avatarBarcode)) {
                                writer.Write(response);

                                using (var message = FusionMessage.Create(NativeMessageTag.ConnectionResponse, writer)) {
                                    MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                                }
                            }
                        }

                        // Now we send all of our other players to the new player
                        foreach (var id in PlayerIdManager.PlayerIds) {
                            var barcode = AvatarWarehouseUtilities.INVALID_AVATAR_BARCODE;
                            if (id.SmallId == 0)
                                barcode = RigData.RigAvatarId;
                            else if (PlayerRep.Representations.ContainsKey(id.SmallId))
                                barcode = PlayerRep.Representations[id.SmallId].avatarId;

                            using (FusionWriter writer = FusionWriter.Create()) {
                                using (var response = ConnectionResponseData.Create(id.LongId, id.SmallId, id.Username, barcode)) {
                                    writer.Write(response);

                                    using (var message = FusionMessage.Create(NativeMessageTag.ConnectionResponse, writer)) {
                                        MessageSender.SendServerMessage(data.longId, NetworkChannel.Reliable, message);
                                    }
                                }
                            }
                        }

                        // Now, make sure the player loads into the scene
                        using (FusionWriter writer = FusionWriter.Create()) {
                            using (var loadData = SceneLoadData.Create(LevelWarehouseUtilities.GetCurrentLevel().Barcode)) {
                                writer.Write(loadData);

                                using (var message = FusionMessage.Create(NativeMessageTag.SceneLoad, writer)) {
                                    MessageSender.SendServerMessage(data.longId, NetworkChannel.Reliable, message);
                                }
                            }
                        }

                        // Send the module list
                        using (var writer = FusionWriter.Create()) {
                            using (var assignData = ModuleAssignData.Create()) {
                                writer.Write(assignData);

                                using (var message = FusionMessage.Create(NativeMessageTag.ModuleAssignment, writer)) {
                                    MessageSender.SendServerMessage(data.longId, NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
