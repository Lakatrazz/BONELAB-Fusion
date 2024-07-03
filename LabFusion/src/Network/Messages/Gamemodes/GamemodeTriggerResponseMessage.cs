using LabFusion.Data;

using LabFusion.SDK.Gamemodes;

namespace LabFusion.Network;

public class GamemodeTriggerResponseData : IFusionSerializable
{
    public ushort gamemodeId;

    public string triggerName;

    public bool hasValue;
    public string triggerValue;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(gamemodeId);
        writer.Write(triggerName);

        writer.Write(hasValue);

        if (hasValue)
        {
            writer.Write(triggerValue);
        }
    }

    public void Deserialize(FusionReader reader)
    {
        gamemodeId = reader.ReadUInt16();
        triggerName = reader.ReadString();

        hasValue = reader.ReadBoolean();

        if (hasValue)
        {
            triggerValue = reader.ReadString();
        }
    }

    public static GamemodeTriggerResponseData Create(ushort gamemodeId, string name, string value = null)
    {
        return new GamemodeTriggerResponseData()
        {
            gamemodeId = gamemodeId,
            triggerName = name,
            hasValue = value != null,
            triggerValue = value,
        };
    }
}

public class GamemodeTriggerResponseMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.GamemodeTriggerResponse;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
            return;
        }

        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<GamemodeTriggerResponseData>();

        if (!GamemodeManager.TryGetGamemode(data.gamemodeId, out var gamemode))
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