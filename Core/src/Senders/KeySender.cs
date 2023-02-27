using LabFusion.Network;
using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Senders {
    public static class KeySender {
        public static void SendStaticKeySlot(ushort keyId, GameObject receiver)
        {
            using (var writer = FusionWriter.Create(KeySlotData.Size))
            {
                using (var data = KeySlotData.Create(PlayerIdManager.LocalSmallId, KeySlotType.INSERT_STATIC, keyId, receiver))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.KeySlot, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendPropKeySlot(ushort keyId, ushort receiverId, byte receiverIndex)
        {
            using (var writer = FusionWriter.Create(KeySlotData.Size))
            {
                using (var data = KeySlotData.Create(PlayerIdManager.LocalSmallId, KeySlotType.INSERT_PROP, keyId, null, receiverId, receiverIndex))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.KeySlot, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
