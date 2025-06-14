using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class GamemodeMetadataRemoveData : INetSerializable
{
    public string gamemodeBarcode;
    public string key;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref gamemodeBarcode);
        serializer.SerializeValue(ref key);
    }

    public static GamemodeMetadataRemoveData Create(string gamemodeBarcode, string key)
    {
        return new GamemodeMetadataRemoveData()
        {
            gamemodeBarcode = gamemodeBarcode,
            key = key,
        };
    }
}

public class GamemodeMetadataRemoveMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.GamemodeMetadataRemove;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GamemodeMetadataRemoveData>();

        if (GamemodeManager.TryGetGamemode(data.gamemodeBarcode, out var gamemode))
        {
            gamemode.Metadata.ForceRemoveLocalMetadata(data.key);
        }
        else
        {
#if DEBUG
            FusionLogger.Warn($"Failed to find a Gamemode with barcode {data.gamemodeBarcode}!");
#endif
        }
    }
}
