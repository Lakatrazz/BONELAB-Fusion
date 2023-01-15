using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class LoadSender {
        public static void SendLevelLoad(string barcode, ulong userId)
        {
            if (!NetworkInfo.IsServer)
                return;

            using (FusionWriter writer = FusionWriter.Create())
            {
                using (var data = SceneLoadData.Create(barcode))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.SceneLoad, writer))
                    {
                        MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendLevelLoad(string barcode) {
            if (!NetworkInfo.IsServer)
                return;

            using (FusionWriter writer = FusionWriter.Create())
            {
                using (var data = SceneLoadData.Create(barcode))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.SceneLoad, writer))
                    {
                        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
