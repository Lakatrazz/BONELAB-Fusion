using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using LabFusion.Player;

namespace LabFusion.Network;

public class PlayerMetadataResponseData : INetSerializable
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

    public static PlayerMetadataResponseData Create(byte smallId, string key, string value)
    {
        return new PlayerMetadataResponseData()
        {
            smallId = smallId,
            key = key,
            value = value,
        };
    }
}

public class PlayerMetadataResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerMetadataResponse;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerMetadataResponseData>();

        var playerId = PlayerIDManager.GetPlayerID(data.smallId);

        playerId?.Metadata.Metadata.ForceSetLocalMetadata(data.key, data.value);
    }
}