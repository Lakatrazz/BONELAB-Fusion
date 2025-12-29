using LabFusion.Network.Serialization;

using LabFusion.SDK.Gamemodes;

namespace LabFusion.Network;

public class GamemodeTriggerResponseData : INetSerializable
{
    public string GamemodeBarcode;

    public string TriggerName;

    public string TriggerValue;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref GamemodeBarcode);
        serializer.SerializeValue(ref TriggerName);
        serializer.SerializeValue(ref TriggerValue);
    }
}

public class GamemodeTriggerResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.GamemodeTriggerResponse;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GamemodeTriggerResponseData>();

        if (!GamemodeManager.TryGetGamemode(data.GamemodeBarcode, out var gamemode))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(data.TriggerValue))
        {
            gamemode.Relay.ForceInvokeLocalTrigger(data.TriggerName, data.TriggerValue);
        }
        else
        {
            gamemode.Relay.ForceInvokeLocalTrigger(data.TriggerName);
        }
    }
}