﻿using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Senders
{
    public static class PermissionSender
    {
        public static void SendPermissionRequest(PermissionCommandType type, byte? otherPlayer = null)
        {
            using FusionWriter writer = FusionWriter.Create();
            var data = PermissionCommandRequestData.Create(PlayerIdManager.LocalSmallId, type, otherPlayer);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PermissionCommandRequest, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }
}
