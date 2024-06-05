using LabFusion.Network;
using LabFusion.Representation;

namespace LabFusion.Senders
{
    public static class HolodeckSender
    {
        public static void SendHolodeckEvent(HolodeckEventType type, int selectionIndex = 0, bool toggleValue = false)
        {
            if (NetworkInfo.HasServer)
            {
                using var writer = FusionWriter.Create();
                var data = HolodeckEventData.Create(PlayerIdManager.LocalSmallId, type, selectionIndex, toggleValue);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.HolodeckEvent, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);
            }
        }
    }
}
