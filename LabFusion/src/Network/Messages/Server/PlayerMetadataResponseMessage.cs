using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Extensions;
using LabFusion.Player;

namespace LabFusion.Network;

public class PlayerMetadataResponseData : IFusionSerializable
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

    public override ExpectedType ExpectedReceiver => ExpectedType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerMetadataResponseData>();

        var playerId = PlayerIdManager.GetPlayerId(data.smallId);

        if (playerId != null)
        {
            playerId.Metadata.ForceSetLocalMetadata(data.key, data.value);
        }
    }
}