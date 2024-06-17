using LabFusion.Network;

namespace LabFusion.Senders
{
    public static class PowerableSender
    {
        public static void SendFunicularControllerEvent(ushort syncId, FunicularControllerEventType type)
        {
            if (NetworkInfo.IsServer)
            {
                using var writer = FusionWriter.Create(FunicularControllerEventData.Size);
                var data = FunicularControllerEventData.Create(syncId, type);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.FunicularControllerEvent, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
        }
    }
}
