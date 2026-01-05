using LabFusion.Network.Serialization;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class GamemodeMetadataRemoveData : INetSerializable
{
    public string GamemodeBarcode;
    public string Key;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref GamemodeBarcode);
        serializer.SerializeValue(ref Key);
    }
}

public class GamemodeMetadataRemoveMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.GamemodeMetadataRemove;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GamemodeMetadataRemoveData>();

        if (GamemodeManager.TryGetGamemode(data.GamemodeBarcode, out var gamemode))
        {
            gamemode.Metadata.ForceRemoveLocalMetadata(data.Key);
        }
        else
        {
#if DEBUG
            FusionLogger.Warn($"Failed to find a Gamemode with barcode {data.GamemodeBarcode}!");
#endif
        }
    }
}
