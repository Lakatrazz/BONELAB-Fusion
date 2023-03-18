using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class CampaignSender {
        public static void SendKartRaceEvent(KartRaceEventType type)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = KartRaceEventData.Create(PlayerIdManager.LocalSmallId, type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.KartRaceEvent, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

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

        public static void SendDescentIntro(DescentIntroEvent introEvent, ulong? target = null)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = DescentIntroData.Create(PlayerIdManager.LocalSmallId, (byte)introEvent.selectionNumber, introEvent.type)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.DescentIntro, writer)) {
                        if (target.HasValue)
                            MessageSender.SendFromServer(target.Value, NetworkChannel.Reliable, message);
                        else
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendDescentNoose(DescentNooseEvent nooseEvent, ulong? target = null) {
            using (var writer = FusionWriter.Create())
            {
                using (var data = DescentNooseData.Create(nooseEvent.smallId, nooseEvent.type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.DescentNoose, writer))
                    {
                        if (target.HasValue)
                            MessageSender.SendFromServer(target.Value, NetworkChannel.Reliable, message);
                        else
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendDescentElevator(DescentElevatorEvent elevatorEvent, ulong? target = null)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = DescentElevatorData.Create(PlayerIdManager.LocalSmallId, elevatorEvent.type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.DescentElevator, writer))
                    {
                        if (target.HasValue)
                            MessageSender.SendFromServer(target.Value, NetworkChannel.Reliable, message);
                        else
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendHomeEvent(int selectionNumber, HomeEventType type)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = HomeEventData.Create((byte)selectionNumber, type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.HomeEvent, writer))
                    {
                        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
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
