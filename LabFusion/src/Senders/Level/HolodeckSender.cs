using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Senders;

public static class HolodeckSender
{
    public static void SendHolodeckEvent(HolodeckEventType type, int selectionIndex = 0, bool toggleValue = false)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = HolodeckEventData.Create(PlayerIdManager.LocalSmallId, type, selectionIndex, toggleValue);

        MessageRelay.RelayNative(data, NativeMessageTag.HolodeckEvent, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}
