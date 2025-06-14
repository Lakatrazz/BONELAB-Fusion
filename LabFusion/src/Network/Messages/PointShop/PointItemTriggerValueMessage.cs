using LabFusion.SDK.Points;
using LabFusion.Player;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PointItemTriggerValueData : INetSerializable
{
    public const int DefaultSize = sizeof(byte);

    public byte smallId;
    public string barcode;
    public string value;

    public static int GetSize(string barcode, string value)
    {
        return DefaultSize + barcode.GetSize() + value.GetSize();
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref barcode);
        serializer.SerializeValue(ref value);
    }

    public static PointItemTriggerValueData Create(byte smallId, string barcode, string value)
    {
        return new PointItemTriggerValueData()
        {
            smallId = smallId,
            barcode = barcode,
            value = value,
        };
    }
}

public class PointItemTriggerValueMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PointItemTriggerValue;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PointItemTriggerValueData>();

        var id = PlayerIDManager.GetPlayerID(data.smallId);
        PointItemManager.Internal_OnTriggerItem(id, data.barcode, data.value);
    }
}
