using LabFusion.Network;
using LabFusion.Representation;

namespace LabFusion.Senders
{
    public static class PointItemSender
    {
        public static void SendPointItemEquip(string barcode, bool isEquipped) {
            if (!NetworkInfo.HasServer)
                return;

            using (var writer = FusionWriter.Create())
            {
                using (var data = PointItemEquipStateData.Create(PlayerIdManager.LocalSmallId, barcode, isEquipped))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PointItemEquipState, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendPointItemTrigger(string barcode)
        {
            if (!NetworkInfo.HasServer)
                return;

            using (var writer = FusionWriter.Create())
            {
                using (var data = PointItemTriggerData.Create(PlayerIdManager.LocalSmallId, barcode))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PointItemTrigger, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendPointItemTrigger(string barcode, string value)
        {
            if (!NetworkInfo.HasServer)
                return;

            using (var writer = FusionWriter.Create())
            {
                using (var data = PointItemTriggerValueData.Create(PlayerIdManager.LocalSmallId, barcode, value))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PointItemTriggerValue, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
