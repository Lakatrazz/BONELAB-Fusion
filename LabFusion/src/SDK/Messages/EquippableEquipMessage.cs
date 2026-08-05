using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Equippables;
using LabFusion.SDK.Modules;

namespace LabFusion.SDK.Messages;

public class EquippableEquipData : INetSerializable
{
    public string Barcode;

    public bool IsEquipped;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Barcode);
        serializer.SerializeValue(ref IsEquipped);
    }
}

public class EquippableEquipMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var sender = received.SenderSmallID;

        if (!sender.HasValue)
        {
            return;
        }

        var data = received.ReadData<EquippableEquipData>();

        var playerID = PlayerIDManager.GetPlayerID(sender.Value);
        
        if (playerID != null)
        {
            EquippableManager.ProcessNetEquip(playerID, data.Barcode, data.IsEquipped);
        }
    }
}
