using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class GamemodeMetadataRemoveData : IFusionSerializable
{
    public string gamemodeBarcode;
    public string key;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(gamemodeBarcode);
        writer.Write(key);
    }

    public void Deserialize(FusionReader reader)
    {
        gamemodeBarcode = reader.ReadString();
        key = reader.ReadString();
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

public class GamemodeMetadataRemoveMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.GamemodeMetadataRemove;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (isServerHandled)
        {
            throw new ExpectedClientException();
        }

        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<GamemodeMetadataRemoveData>();

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
