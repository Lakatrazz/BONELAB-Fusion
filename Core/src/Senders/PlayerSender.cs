using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using SLZ.Zones;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public enum PlayerActionType {
        UNKNOWN = 1 << 0,
        JUMP = 1 << 1,
        DEATH = 1 << 2,
        DYING = 1 << 3,
        RECOVERY = 1 << 4,
    }

    public static class PlayerSender {
        public static void SendPlayerAction(PlayerActionType type) {
            using (var writer = FusionWriter.Create()) {
                using (var data = PlayerRepActionData.Create(PlayerIdManager.LocalSmallId, type)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepAction, writer)) {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }

            // Inform the hooks locally
            MultiplayerHooking.Internal_OnPlayerAction(PlayerIdManager.LocalId, type);
        }
    }
}
