﻿using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Senders
{
    public enum TimeScaleMode
    {
        DISABLED = 0,
        LOW_GRAVITY = 1,
        HOST_ONLY = 2,
        EVERYONE = 3,
        CLIENT_SIDE_UNSTABLE = 4,
    }

    public static class TimeScaleSender
    {
        public static void SendSlowMoButton(bool isDecrease)
        {
            using var writer = FusionWriter.Create(SlowMoButtonMessageData.Size);
            var data = SlowMoButtonMessageData.Create(PlayerIdManager.LocalSmallId, isDecrease);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.SlowMoButton, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }
}
