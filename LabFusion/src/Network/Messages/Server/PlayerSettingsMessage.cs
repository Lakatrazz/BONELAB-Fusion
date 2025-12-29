using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PlayerSettingsData : INetSerializable
{
    public const int Size = SerializedPlayerSettings.Size;

    public SerializedPlayerSettings Settings;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Settings);
    }
}

public class PlayerSettingsMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerSettings;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerSettingsData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(sender.Value, out var player))
        {
            player.SetSettings(data.Settings);
        }
    }
}