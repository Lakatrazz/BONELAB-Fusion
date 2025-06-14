using LabFusion.Data;
using LabFusion.SDK.Points;
using LabFusion.Player;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PointItemTriggerData : INetSerializable
{
    public byte smallId;
    public string barcode;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref barcode);
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

        var id = PlayerIDManager.GetPlayerID(data.smallId);
        PointItemManager.Internal_OnTriggerItem(id, data.barcode);
    }
}
