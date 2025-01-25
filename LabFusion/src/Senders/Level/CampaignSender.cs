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

    public static void SendDescentIntro(DescentIntroEvent introEvent, ulong? target = null)
    {
        using var writer = FusionWriter.Create();
        var data = DescentIntroData.Create(PlayerIdManager.LocalSmallId, (byte)introEvent.selectionNumber, introEvent.type);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.DescentIntro, writer);
        if (target.HasValue)
            MessageSender.SendFromServer(target.Value, NetworkChannel.Reliable, message);
        else
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }

    public static void SendDescentNoose(DescentNooseEvent nooseEvent, ulong? target = null)
    {
        using var writer = FusionWriter.Create();
        var data = DescentNooseData.Create(nooseEvent.smallId, nooseEvent.type);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.DescentNoose, writer);

        if (target.HasValue)
        {
            MessageSender.SendFromServer(target.Value, NetworkChannel.Reliable, message);
        }
        else
        {
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }

    public static void SendDescentElevator(DescentElevatorEvent elevatorEvent, ulong? target = null)
    {
        using var writer = FusionWriter.Create();
        var data = DescentElevatorData.Create(PlayerIdManager.LocalSmallId, elevatorEvent.type);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.DescentElevator, writer);

        if (target.HasValue)
        {
            MessageSender.SendFromServer(target.Value, NetworkChannel.Reliable, message);
        }
        else
        {
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }

    public static void SendHomeEvent(int selectionNumber, HomeEventType type)
    {
        var data = HomeEventData.Create((byte)selectionNumber, type);

        MessageRelay.RelayNative(data, NativeMessageTag.HomeEvent, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }

    public static void SendMagmaGateEvent(MagmaGateEventType type)
    {
        using var writer = FusionWriter.Create();
        var data = MagmaGateEventData.Create(type);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.MagmaGateEvent, writer);
        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
    }
}
