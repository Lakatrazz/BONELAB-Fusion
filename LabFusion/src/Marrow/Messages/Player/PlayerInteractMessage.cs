using LabFusion.Marrow.Player;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Modules;

namespace LabFusion.Marrow.Messages;

public class PlayerInteractData : INetSerializable
{
    public const int Size = PlayerReference.Size + sizeof(byte);

    public PlayerReference OtherPlayerReference;

    public PlayerInteractType Type;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref OtherPlayerReference);
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

public class PlayerInteractMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerInteractData>();

        var sender = received.Sender.Value;

        var playerID = PlayerIDManager.GetPlayerID(sender);

        if (playerID == null)
        {
            return;
        }

        if (!data.OtherPlayerReference.TryGetPlayer(out var otherPlayerID))
        {
            return;
        }

        PlayerInteractManager.OnPlayerInteraction(playerID, otherPlayerID, data.Type);
    }
}
