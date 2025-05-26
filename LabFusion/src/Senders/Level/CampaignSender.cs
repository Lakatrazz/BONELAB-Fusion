using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Senders;

public static class CampaignSender
{
    public static void SendKartRaceEvent(KartRaceEventType type)
    {
        var data = KartRaceEventData.Create(PlayerIdManager.LocalSmallId, type);

        MessageRelay.RelayNative(data, NativeMessageTag.KartRaceEvent, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }

    public static void SendHomeEvent(int selectionNumber, HomeEventType type)
    {
        var data = HomeEventData.Create((byte)selectionNumber, type);

        MessageRelay.RelayNative(data, NativeMessageTag.HomeEvent, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }

    public static void SendMagmaGateEvent(MagmaGateEventType type)
    {
        var data = MagmaGateEventData.Create(type);

        MessageRelay.RelayNative(data, NativeMessageTag.MagmaGateEvent, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}
