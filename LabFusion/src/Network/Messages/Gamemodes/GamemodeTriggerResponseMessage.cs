using LabFusion.Data;

using LabFusion.SDK.Gamemodes;

namespace LabFusion.Network;

public class GamemodeTriggerResponseData : IFusionSerializable
{
    public string gamemodeBarcode;

    public string triggerName;

    public bool hasValue;
    public string triggerValue;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(gamemodeBarcode);
        writer.Write(triggerName);

        writer.Write(hasValue);

        if (hasValue)
        {
            writer.Write(triggerValue);
        }
    }

    public void Deserialize(FusionReader reader)
    {
        gamemodeBarcode = reader.ReadString();
        triggerName = reader.ReadString();

        hasValue = reader.ReadBoolean();

        if (hasValue)
        {
            triggerValue = reader.ReadString();
        }
    }

    public static GamemodeTriggerResponseData Create(string gamemodeBarcode, string name, string value = null)
    {
        return new GamemodeTriggerResponseData()
        {
            gamemodeBarcode = gamemodeBarcode,
            triggerName = name,
            hasValue = value != null,
            triggerValue = value,
        };
    }
}

public class GamemodeTriggerResponseMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.GamemodeTriggerResponse;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
            return;
        }

        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<GamemodeTriggerResponseData>();

        if (!GamemodeManager.TryGetGamemode(data.gamemodeBarcode, out var gamemode))
        {
            return;
        }

        if (data.hasValue)
        {
            gamemode.Relay.ForceInvokeLocalTrigger(data.triggerName, data.triggerValue);
        }
        else
        {
            gamemode.Relay.ForceInvokeLocalTrigger(data.triggerName);
        }
    }
}