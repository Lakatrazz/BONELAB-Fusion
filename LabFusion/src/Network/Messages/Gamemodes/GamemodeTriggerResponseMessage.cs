using LabFusion.Network.Serialization;

using LabFusion.SDK.Gamemodes;

namespace LabFusion.Network;

public class GamemodeTriggerResponseData : INetSerializable
{
    public string gamemodeBarcode;

    public string triggerName;

    public string triggerValue;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref gamemodeBarcode);
        serializer.SerializeValue(ref triggerName);
        serializer.SerializeValue(ref triggerValue);
    }

    public static GamemodeTriggerResponseData Create(string gamemodeBarcode, string name, string value = null)
    {
        return new GamemodeTriggerResponseData()
        {
            gamemodeBarcode = gamemodeBarcode,
            triggerName = name,
            triggerValue = value,
        };
    }
}

public class GamemodeTriggerResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.GamemodeTriggerResponse;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GamemodeTriggerResponseData>();

        if (!GamemodeManager.TryGetGamemode(data.gamemodeBarcode, out var gamemode))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(data.triggerValue))
        {
            gamemode.Relay.ForceInvokeLocalTrigger(data.triggerName, data.triggerValue);
        }
        else
        {
            gamemode.Relay.ForceInvokeLocalTrigger(data.triggerName);
        }
    }
}