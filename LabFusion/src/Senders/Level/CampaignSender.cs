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

    public static void SendHubEvent(BonelabHubEventType type)
    {
        var data = BonelabHubEventData.Create(type);

        MessageRelay.RelayNative(data, NativeMessageTag.BonelabHubEvent, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }

    public static void SendDescentIntro(DescentIntroEvent introEvent, PlayerId target = null)
    {
        var data = DescentIntroData.Create(PlayerIdManager.LocalSmallId, (byte)introEvent.selectionNumber, introEvent.type);

        if (target != null)
        {
            MessageRelay.RelayNative(data, NativeMessageTag.DescentIntro, NetworkChannel.Reliable, RelayType.ToTarget, target.SmallId);
        }
        else
        {
            MessageRelay.RelayNative(data, NativeMessageTag.DescentIntro, NetworkChannel.Reliable, RelayType.ToOtherClients);
        }
    }

    public static void SendDescentNoose(DescentNooseEvent nooseEvent, PlayerId? target = null)
    {
        var data = DescentNooseData.Create(nooseEvent.smallId, nooseEvent.type);

        if (target != null)
        {
            MessageRelay.RelayNative(data, NativeMessageTag.DescentNoose, NetworkChannel.Reliable, RelayType.ToTarget, target.SmallId);
        }
        else
        {
            MessageRelay.RelayNative(data, NativeMessageTag.DescentNoose, NetworkChannel.Reliable, RelayType.ToOtherClients);
        }
    }

    public static void SendDescentElevator(DescentElevatorEvent elevatorEvent, PlayerId? target = null)
    {
        var data = DescentElevatorData.Create(PlayerIdManager.LocalSmallId, elevatorEvent.type);

        if (target != null)
        {
            MessageRelay.RelayNative(data, NativeMessageTag.DescentElevator, NetworkChannel.Reliable, RelayType.ToTarget, target.SmallId);
        }
        else
        {
            MessageRelay.RelayNative(data, NativeMessageTag.DescentElevator, NetworkChannel.Reliable, RelayType.ToOtherClients);
        }
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
