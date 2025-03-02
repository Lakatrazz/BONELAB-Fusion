using LabFusion.Network;
using Il2CppSLZ.Bonelab;

namespace LabFusion.Senders
{
    public static class TrialSender
    {
        public static void SendTimeTrialEvent(TimeTrialGameControllerType type, int value)
        {
            if (NetworkInfo.IsServer)
            {
                var data = TimeTrialGameControllerData.Create(type, value);

                MessageRelay.RelayNative(data, NativeMessageTag.TimeTrial_GameController, NetworkChannel.Reliable, RelayType.ToOtherClients);
            }
        }

        public static void SendTrialSpawnerEvent(Trial_SpawnerEvents spawnerEvent)
        {
            if (!NetworkInfo.IsServer)
            {
                return;
            }

            var data = TrialSpawnerEventsData.Create(spawnerEvent);

            MessageRelay.RelayNative(data, NativeMessageTag.TrialSpawnerEvents, NetworkChannel.Reliable, RelayType.ToOtherClients);
        }
    }
}
