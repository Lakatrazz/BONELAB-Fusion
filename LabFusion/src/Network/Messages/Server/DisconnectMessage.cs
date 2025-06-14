using LabFusion.Network.Serialization;

using LabFusion.Player;

namespace LabFusion.Network;

public class DisconnectMessageData : INetSerializable
{
    public ulong longId;
    public string reason;

    public static DisconnectMessageData Create(ulong longId, string reason = "")
    {
        return new DisconnectMessageData()
        {
            longId = longId,
            reason = reason,
        };
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref longId);
        serializer.SerializeValue(ref reason);
    }
}

public class DisconnectMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.Disconnect;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DisconnectMessageData>();

        // If this is our id, disconnect ourselves
        if (data.longId == PlayerIDManager.LocalPlatformID)
        {
            NetworkHelper.Disconnect(data.reason);
        }
        // Otherwise, disconnect the other person in the lobby
        else
        {
            InternalServerHelpers.OnPlayerLeft(data.longId);
        }
    }
}
