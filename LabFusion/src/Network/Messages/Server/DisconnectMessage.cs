using LabFusion.Network.Serialization;

using LabFusion.Player;

namespace LabFusion.Network;

public class DisconnectMessageData : INetSerializable
{
    public ulong PlatformID;
    public string Reason;

    public static DisconnectMessageData Create(ulong longId, string reason = "")
    {
        return new DisconnectMessageData()
        {
            PlatformID = longId,
            Reason = reason,
        };
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlatformID);
        serializer.SerializeValue(ref Reason);
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
        if (data.PlatformID == PlayerIDManager.LocalPlatformID)
        {
            NetworkHelper.Disconnect(data.Reason);
        }
        // Otherwise, disconnect the other person in the lobby
        else
        {
            InternalServerHelpers.OnPlayerLeft(data.PlatformID);
        }
    }
}
