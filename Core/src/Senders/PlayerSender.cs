using LabFusion.Network;
using LabFusion.Representation;

using SLZ.Zones;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class PlayerSender {
        public static void SendPlayerRepEvent(PlayerRepEventType type) {
            using (var writer = FusionWriter.Create()) {
                using (var data = PlayerRepEventData.Create(PlayerIdManager.LocalSmallId, type)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepEvent, writer)) {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

    }
}
