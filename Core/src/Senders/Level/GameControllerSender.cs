using LabFusion.Network;
using SLZ.Bonelab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders
{
    public static class GameControllerSender
    {
        public static void SendGameControllerEvent(BaseGameControllerType type)
        {
            if (NetworkInfo.IsServer)
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = BaseGameControllerData.Create(type))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.BaseGameController, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
