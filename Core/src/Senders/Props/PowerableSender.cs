using LabFusion.Network;
using LabFusion.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class PowerableSender
    {
        public static void SendPowerableJointVoltage(ushort syncId, float voltage)
        {
            if (!NetworkInfo.HasServer)
                return;

            using (var writer = FusionWriter.Create(PowerableJointVoltageData.Size))
            {
                using (var data = PowerableJointVoltageData.Create(PlayerIdManager.LocalSmallId, syncId, voltage))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PowerableJointVoltage, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendTwoButtonRemoteControllerEvent(ushort syncId, TwoButtonRemoteControllerEventType type) {
            if (!NetworkInfo.HasServer)
                return;

            using (var writer = FusionWriter.Create(TwoButtonRemoteControllerEventData.Size))
            {
                using (var data = TwoButtonRemoteControllerEventData.Create(PlayerIdManager.LocalSmallId, syncId, type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.TwoButtonRemoteControllerEvent, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendFunicularControllerEvent(ushort syncId, FunicularControllerEventType type)
        {
            if (NetworkInfo.IsServer)
            {
                using (var writer = FusionWriter.Create(FunicularControllerEventData.Size))
                {
                    using (var data = FunicularControllerEventData.Create(syncId, type))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.FunicularControllerEvent, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
