using LabFusion.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class FunicularSender
    {
        public static void SendFunicularControllerEvent(ushort syncId, FunicularControllerEventType type)
        {
            if (NetworkInfo.IsServer)
            {
                using (var writer = FusionWriter.Create())
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
