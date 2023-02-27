using LabFusion.Exceptions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class PullCordSender {
        public static void SendBodyLogToggle(bool isEnabled)
        {
            if (!NetworkInfo.HasServer)
                return;

            using (var writer = FusionWriter.Create(BodyLogToggleData.Size))
            {
                using (var data = BodyLogToggleData.Create(PlayerIdManager.LocalSmallId, isEnabled))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.BodyLogToggle, writer))
                    {
                        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendBodyLogEffect() {
            if (!NetworkInfo.HasServer)
                return;

            using (var writer = FusionWriter.Create(BodyLogEffectData.Size)) {
                using (var data = BodyLogEffectData.Create(PlayerIdManager.LocalSmallId)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.BodyLogEffect, writer)) {
                        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
