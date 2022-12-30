using LabFusion.Network;
using LabFusion.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class CampaignSender {
        public static void SendHubEvent(BonelabHubEventType type)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = BonelabHubEventData.Create(type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.BonelabHubEvent, writer))
                    {
                        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendDescentIntro(int selectionNumber, DescentIntroType type)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = DescentIntroData.Create(PlayerIdManager.LocalSmallId, (byte)selectionNumber, type)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.DescentIntro, writer)) {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendDescentNoose(DescentNooseType type) {
            using (var writer = FusionWriter.Create())
            {
                using (var data = DescentNooseData.Create(PlayerIdManager.LocalSmallId, type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.DescentNoose, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendDescentElevator(DescentElevatorType type)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = DescentElevatorData.Create(PlayerIdManager.LocalSmallId, type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.DescentElevator, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendMagmaGateEvent(MagmaGateEventType type)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = MagmaGateEventData.Create(type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.MagmaGateEvent, writer))
                    {
                        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
