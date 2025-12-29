using LabFusion.Network.Serialization;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.Network;

public class GamemodeMetadataSetData : INetSerializable
{
    public string GamemodeBarcode;
    public string Key;
    public string Value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref GamemodeBarcode);
        serializer.SerializeValue(ref Key);
        serializer.SerializeValue(ref Value);
    }

    public static GamemodeMetadataSetData Create(string gamemodeBarcode, string key, string value)
    {
        return new GamemodeMetadataSetData()
        {
            GamemodeBarcode = gamemodeBarcode,
            Key = key,
            Value = value,
        };
    }
}

public class GamemodeMetadataSetMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.GamemodeMetadataSet;

    public override ExpectedSenderType ExpectedSender => ExpectedSenderType.ServerOnly;
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GamemodeMetadataSetData>();

        if (GamemodeManager.TryGetGamemode(data.GamemodeBarcode, out var gamemode))
        {
            gamemode.Metadata.ForceSetLocalMetadata(data.Key, data.Value);
        }
        else
        {
#if DEBUG
            FusionLogger.Warn($"Failed to find a Gamemode with barcode {data.gamemodeBarcode}!");
#endif
        }
    }
}