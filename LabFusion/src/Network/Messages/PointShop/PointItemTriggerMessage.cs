using LabFusion.Data;
using LabFusion.SDK.Points;
using LabFusion.Player;

namespace LabFusion.Network;

public class PointItemTriggerData : IFusionSerializable
{
    public byte smallId;
    public string barcode;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(barcode);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        barcode = reader.ReadString();
    }

    public static PointItemTriggerData Create(byte smallId, string barcode)
    {
        return new PointItemTriggerData()
        {
            smallId = smallId,
            barcode = barcode,
        };
    }
}

public class PointItemTriggerMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PointItemTrigger;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PointItemTriggerData>();

        var id = PlayerIdManager.GetPlayerId(data.smallId);
        PointItemManager.Internal_OnTriggerItem(id, data.barcode);
    }
}
