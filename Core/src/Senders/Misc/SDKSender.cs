using LabFusion.Data;
using LabFusion.MarrowIntegration;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

namespace LabFusion.Senders {
    public static class SDKSender {
        public static void SendGameObjectActive(bool value, SyncGameObjectEnabled script) {
            using (FusionWriter writer = FusionWriter.Create()) {
                using (var data = GameObjectActiveData.Create(PlayerIdManager.LocalSmallId, value, script)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.GameObjectActive, writer)) {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
