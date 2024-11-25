using LabFusion.Exceptions;
using LabFusion.Network;

namespace LabFusion.Senders;

public static class GamemodeSender
{
    public static void SendGamemodeTriggerResponse(string gamemodeBarcode, string name, string value = null)
    {
        using var writer = FusionWriter.Create();
        var data = GamemodeTriggerResponseData.Create(gamemodeBarcode, name, value);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.GamemodeTriggerResponse, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }

    public static void SendGamemodeMetadataSet(string gamemodeBarcode, string key, string value)
    {
        // Make sure this is the server
        if (!NetworkInfo.IsServer)
        {
            throw new ExpectedServerException();
        }

        using var writer = FusionWriter.Create();
        var data = GamemodeMetadataSetData.Create(gamemodeBarcode, key, value);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.GamemodeMetadataSet, writer);
        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
    }

    public static void SendGamemodeMetadataRemove(string gamemodeBarcode, string key)
    {
        // Make sure this is the server
        if (!NetworkInfo.IsServer)
        {
            throw new ExpectedServerException();
        }

        using var writer = FusionWriter.Create();
        var data = GamemodeMetadataRemoveData.Create(gamemodeBarcode, key);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.GamemodeMetadataRemove, writer);
        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
    }
}