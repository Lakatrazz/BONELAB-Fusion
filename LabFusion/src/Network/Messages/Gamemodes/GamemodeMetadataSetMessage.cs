using LabFusion.Network.Serialization;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class GamemodeMetadataSetData : INetSerializable
{
    public string gamemodeBarcode;
    public string key;
    public string value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref gamemodeBarcode);
        serializer.SerializeValue(ref key);
        serializer.SerializeValue(ref value);
    }

    public static GamemodeMetadataSetData Create(string gamemodeBarcode, string key, string value)
    {
        return new GamemodeMetadataSetData()
        {
            gamemodeBarcode = gamemodeBarcode,
            key = key,
            value = value,
        };
    }
}

public class GamemodeMetadataSetMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.GamemodeMetadataSet;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GamemodeMetadataSetData>();

        if (GamemodeManager.TryGetGamemode(data.gamemodeBarcode, out var gamemode))
        {
            gamemode.Metadata.ForceSetLocalMetadata(data.key, data.value);
        }
        else
        {
#if DEBUG
            FusionLogger.Warn($"Failed to find a Gamemode with barcode {data.gamemodeBarcode}!");
#endif
        }
    }
}