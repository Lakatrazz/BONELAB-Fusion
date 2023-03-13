using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

namespace LabFusion.Senders {
    public static class PermissionSender {
        public static void SendPermissionRequest(PermissionCommandType type, byte? otherPlayer = null) {
            using (FusionWriter writer = FusionWriter.Create()) {
                using (var data = PermissionCommandRequestData.Create(PlayerIdManager.LocalSmallId, type, otherPlayer)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PermissionCommandRequest, writer)) {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
