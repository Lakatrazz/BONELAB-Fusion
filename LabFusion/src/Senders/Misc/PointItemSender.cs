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

        var data = PointItemEquipStateData.Create(PlayerIDManager.LocalSmallID, barcode, isEquipped);

        MessageRelay.RelayNative(data, NativeMessageTag.PointItemEquipState, CommonMessageRoutes.ReliableToOtherClients);
    }

    public static void SendPointItemTrigger(string barcode)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = PointItemTriggerData.Create(PlayerIDManager.LocalSmallID, barcode);

        MessageRelay.RelayNative(data, NativeMessageTag.PointItemTrigger, CommonMessageRoutes.ReliableToOtherClients);
    }

    public static void SendPointItemTrigger(string barcode, string value)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = PointItemTriggerValueData.Create(PlayerIDManager.LocalSmallID, barcode, value);

        MessageRelay.RelayNative(data, NativeMessageTag.PointItemTriggerValue, CommonMessageRoutes.ReliableToOtherClients);
    }
}
