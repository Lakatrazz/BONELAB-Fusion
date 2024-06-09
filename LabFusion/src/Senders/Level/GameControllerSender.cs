using LabFusion.Network;

namespace LabFusion.Senders
{
    public static class GameControllerSender
    {
        public static void SendGameControllerEvent(BaseGameControllerType type)
        {
            if (NetworkInfo.IsServer)
            {
                using var writer = FusionWriter.Create();
                var data = BaseGameControllerData.Create(type);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.BaseGameController, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
        }
    }
}
