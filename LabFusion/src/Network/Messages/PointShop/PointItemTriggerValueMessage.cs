using LabFusion.SDK.Points;
using LabFusion.Player;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PointItemTriggerValueData : INetSerializable
{
    public string Barcode;
    public string Value;

    public int? GetSize() => Barcode.GetSize() + Value.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Barcode);
        serializer.SerializeValue(ref Value);
    }
}

public class PointItemTriggerValueMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PointItemTriggerValue;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PointItemTriggerValueData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        var id = PlayerIDManager.GetPlayerID(sender.Value);
        PointItemManager.Internal_OnTriggerItem(id, data.Barcode, data.Value);
    }
}
