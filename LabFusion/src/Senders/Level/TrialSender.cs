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
                using var writer = FusionWriter.Create();
                var data = TimeTrialGameControllerData.Create(type, value);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.TimeTrial_GameController, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
        }

        public static void SendTrialSpawnerEvent(Trial_SpawnerEvents spawnerEvent)
        {
            if (NetworkInfo.IsServer)
            {
                using var writer = FusionWriter.Create();
                var data = TrialSpawnerEventsData.Create(spawnerEvent);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.TrialSpawnerEvents, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
        }

    }
}
