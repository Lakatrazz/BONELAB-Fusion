using LabFusion.Network;
using LabFusion.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Senders {
    public enum TimeScaleMode {
        DISABLED = 0,
        LOW_GRAVITY = 1,
        HOST_ONLY = 2,
        EVERYONE = 3,
    }

    public static class TimeScaleSender {
        internal static float ReceivedTimeScale = 1f;

        public static void SendSlowMoButton(bool isDecrease)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = SlowMoButtonMessageData.Create(PlayerIdManager.LocalSmallId, isDecrease))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.SlowMoButton, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        internal static void SendTimeScale() {
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create()) {
                    using (var data = TimeScaleMessageData.Create()) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.TimeScale, writer)) {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);
                        }
                    }
                }
            }
        }
    }
}
