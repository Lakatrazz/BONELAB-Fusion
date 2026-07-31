using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class PlayerRepActionData : INetSerializable
{
    public const int Size = sizeof(byte) * 2;

    public PlayerActionType Type;

    public byte? OtherPlayer;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
        serializer.SerializeValue(ref OtherPlayer);
    }
}

public class PlayerRepActionMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepAction;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepActionData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        if (!NetworkPlayerManager.TryGetPlayer(sender.Value, out var player))
        {
            return;
        }

        PlayerID otherPlayer = data.OtherPlayer.HasValue ? PlayerIDManager.GetPlayerID(data.OtherPlayer.Value) : null;

        // Inform the hooks
        MultiplayerHooking.InvokeOnPlayerAction(player.PlayerID, data.Type, otherPlayer);
    }
}
