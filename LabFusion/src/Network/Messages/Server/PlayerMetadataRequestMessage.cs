using LabFusion.Senders;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PlayerMetadataRequestData : INetSerializable
{
    public const int DefaultSize = sizeof(byte);

    public byte smallId;
    public string key;
    public string value;

    public static int GetSize(string key, string value)
    {
        return DefaultSize + key.GetSize() + value.GetSize();
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref key);
        serializer.SerializeValue(ref value);
    }

    public static PlayerMetadataRequestData Create(byte smallId, string key, string value)
    {
        return new PlayerMetadataRequestData()
        {
            smallId = smallId,
            key = key,
            value = value,
        };
    }
}

public class PlayerMetadataRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerMetadataRequest;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerMetadataRequestData>();

        // Send the response to all clients
        PlayerSender.SendPlayerMetadataResponse(data.smallId, data.key, data.value);
    }
}
