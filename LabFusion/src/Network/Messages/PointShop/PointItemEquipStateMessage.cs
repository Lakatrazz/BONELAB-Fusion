using LabFusion.SDK.Points;
using LabFusion.Player;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PointItemEquipStateData : INetSerializable
{
    public string Barcode;

    public bool IsEquipped;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Barcode);
        serializer.SerializeValue(ref IsEquipped);
    }
}

public class PointItemEquipStateMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PointItemEquipState;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PointItemEquipStateData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        if (PointItemManager.TryGetPointItem(data.Barcode, out var item))
        {
            var id = PlayerIDManager.GetPlayerID(sender.Value);

            id.ForceSetEquipped(data.Barcode, data.IsEquipped);
        }
    }
}
