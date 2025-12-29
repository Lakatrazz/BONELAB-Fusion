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

        var data = new PointItemEquipStateData()
        {
            Barcode = barcode,
            IsEquipped = isEquipped,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.PointItemEquipState, CommonMessageRoutes.ReliableToOtherClients);
    }

    public static void SendPointItemTrigger(string barcode)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = new PointItemTriggerData()
        {
            Barcode = barcode,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.PointItemTrigger, CommonMessageRoutes.ReliableToOtherClients);
    }

    public static void SendPointItemTrigger(string barcode, string value)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = new PointItemTriggerValueData()
        {
            Barcode = barcode,
            Value = value,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.PointItemTriggerValue, CommonMessageRoutes.ReliableToOtherClients);
    }
}
