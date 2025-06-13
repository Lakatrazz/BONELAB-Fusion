using LabFusion.Exceptions;
using LabFusion.Network;

namespace LabFusion.Senders;

public static class GamemodeSender
{
    public static void SendGamemodeTriggerResponse(string gamemodeBarcode, string name, string value = null)
    {
        var data = GamemodeTriggerResponseData.Create(gamemodeBarcode, name, value);

        MessageRelay.RelayNative(data, NativeMessageTag.GamemodeTriggerResponse, CommonMessageRoutes.ReliableToClients);
    }

    public static void SendGamemodeMetadataSet(string gamemodeBarcode, string key, string value)
    {
        // Make sure this is the server
        if (!NetworkInfo.IsHost)
        {
            throw new ExpectedServerException();
        }

        var data = GamemodeMetadataSetData.Create(gamemodeBarcode, key, value);

        MessageRelay.RelayNative(data, NativeMessageTag.GamemodeMetadataSet, CommonMessageRoutes.ReliableToClients);
    }

    public static void SendGamemodeMetadataRemove(string gamemodeBarcode, string key)
    {
        // Make sure this is the server
        if (!NetworkInfo.IsHost)
        {
            throw new ExpectedServerException();
        }

        var data = GamemodeMetadataRemoveData.Create(gamemodeBarcode, key);

        MessageRelay.RelayNative(data, NativeMessageTag.GamemodeMetadataRemove, CommonMessageRoutes.ReliableToClients);
    }
}