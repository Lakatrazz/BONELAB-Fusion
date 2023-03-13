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
        CLIENT_SIDE_UNSTABLE = 4,
    }

    public static class TimeScaleSender {
        public static void SendSlowMoButton(bool isDecrease)
        {
            using (var writer = FusionWriter.Create(SlowMoButtonMessageData.Size))
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
    }
}
