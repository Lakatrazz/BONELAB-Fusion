using LabFusion.SDK.Points;
using LabFusion.Player;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PointItemTriggerData : INetSerializable
{
    public string Barcode;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Barcode);
    }
}

public class PointItemTriggerMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PointItemTrigger;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PointItemTriggerData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        var id = PlayerIDManager.GetPlayerID(sender.Value);
        PointItemManager.Internal_OnTriggerItem(id, data.Barcode);
    }
}
