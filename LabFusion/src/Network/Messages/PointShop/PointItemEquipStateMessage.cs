using LabFusion.SDK.Points;
using LabFusion.Player;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PointItemEquipStateData : INetSerializable
{
    public byte smallId;
    public string barcode;
    public bool isEquipped;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref barcode);
        serializer.SerializeValue(ref isEquipped);
    }

    public static PointItemEquipStateData Create(byte smallId, string barcode, bool isEquipped)
    {
        return new PointItemEquipStateData()
        {
            smallId = smallId,
            barcode = barcode,
            isEquipped = isEquipped,
        };
    }
}

public class PointItemEquipStateMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PointItemEquipState;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PointItemEquipStateData>();

        if (PointItemManager.TryGetPointItem(data.barcode, out var item))
        {
            var id = PlayerIDManager.GetPlayerID(data.smallId);

            id.ForceSetEquipped(data.barcode, data.isEquipped);
        }
    }
}
