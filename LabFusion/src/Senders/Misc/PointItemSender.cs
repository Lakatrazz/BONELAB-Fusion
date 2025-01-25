using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Senders;

public static class PointItemSender
{
    public static void SendPointItemEquip(string barcode, bool isEquipped)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = PointItemEquipStateData.Create(PlayerIdManager.LocalSmallId, barcode, isEquipped);

        MessageRelay.RelayNative(data, NativeMessageTag.PointItemEquipState, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }

    public static void SendPointItemTrigger(string barcode)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = PointItemTriggerData.Create(PlayerIdManager.LocalSmallId, barcode);

        MessageRelay.RelayNative(data, NativeMessageTag.PointItemTrigger, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }

    public static void SendPointItemTrigger(string barcode, string value)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = PointItemTriggerValueData.Create(PlayerIdManager.LocalSmallId, barcode, value);

        MessageRelay.RelayNative(data, NativeMessageTag.PointItemTriggerValue, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}
