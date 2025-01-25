using LabFusion.Data;
using LabFusion.Senders;
using LabFusion.Extensions;

namespace LabFusion.Network;

public class PlayerMetadataRequestData : IFusionSerializable
{
    public const int DefaultSize = sizeof(byte);

    public byte smallId;
    public string key;
    public string value;

    public static int GetSize(string key, string value)
    {
        return DefaultSize + key.GetSize() + value.GetSize();
    }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(key);
        writer.Write(value);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        key = reader.ReadString();
        value = reader.ReadString();
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

    public override ExpectedType ExpectedReceiver => ExpectedType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        using FusionReader reader = FusionReader.Create(received.Bytes);
        var data = reader.ReadFusionSerializable<PlayerMetadataRequestData>();

        // Send the response to all clients
        PlayerSender.SendPlayerMetadataResponse(data.smallId, data.key, data.value);
    }
}
