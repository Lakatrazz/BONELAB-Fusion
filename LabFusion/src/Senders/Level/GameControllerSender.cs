using LabFusion.Network;

namespace LabFusion.Senders
{
    public static class GameControllerSender
    {
        public static void SendGameControllerEvent(BaseGameControllerType type)
        {
            if (NetworkInfo.IsServer)
            {
                var data = BaseGameControllerData.Create(type);

                MessageRelay.RelayNative(data, NativeMessageTag.BaseGameController, NetworkChannel.Reliable, RelayType.ToOtherClients);
            }
        }
    }
}
