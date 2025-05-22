using LabFusion.Network;
using Il2CppSLZ.Bonelab;

namespace LabFusion.Senders;

public static class TrialSender
{
    public static void SendTimeTrialEvent(TimeTrialGameControllerType type, int value)
    {
        if (NetworkInfo.IsHost)
        {
            var data = TimeTrialGameControllerData.Create(type, value);

            MessageRelay.RelayNative(data, NativeMessageTag.TimeTrial_GameController, NetworkChannel.Reliable, RelayType.ToOtherClients);
        }
    }
}
