using LabFusion.Exceptions;
using LabFusion.Network;

namespace LabFusion.Senders;

public static class GamemodeSender
{
    public static void SendGamemodeTriggerResponse(string gamemodeBarcode, string name, string value = null)
    {
        var data = new GamemodeTriggerResponseData()
        {
            GamemodeBarcode = gamemodeBarcode,
            TriggerName = name,
            TriggerValue = value,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.GamemodeTriggerResponse, CommonMessageRoutes.ReliableToClients);
    }

    public static void SendGamemodeMetadataSet(string gamemodeBarcode, string key, string value)
    {
        // Make sure this is the server
        if (!NetworkInfo.IsHost)
        {
            throw new ExpectedFromServerException();
        }

        var data = new GamemodeMetadataSetData()
        {
            GamemodeBarcode = gamemodeBarcode,
            Key = key,
            Value = value,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.GamemodeMetadataSet, CommonMessageRoutes.ReliableToClients);
    }

    public static void SendGamemodeMetadataRemove(string gamemodeBarcode, string key)
    {
        // Make sure this is the server
        if (!NetworkInfo.IsHost)
        {
            throw new ExpectedFromServerException();
        }

        var data = new GamemodeMetadataRemoveData()
        {
            GamemodeBarcode = gamemodeBarcode,
            Key = key,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.GamemodeMetadataRemove, CommonMessageRoutes.ReliableToClients);
    }
}