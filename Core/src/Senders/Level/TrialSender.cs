using LabFusion.Network;
using SLZ.Bonelab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders
{
    public static class TrialSender
    {
        public static void SendTimeTrialEvent(TimeTrialGameControllerType type, int value)
        {
            if (NetworkInfo.IsServer)
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = TimeTrialGameControllerData.Create(type, value))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.TimeTrial_GameController, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static void SendTrialSpawnerEvent(Trial_SpawnerEvents spawnerEvent)
        {
            if (NetworkInfo.IsServer)
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = TrialSpawnerEventsData.Create(spawnerEvent))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.TrialSpawnerEvents, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

    }
}
