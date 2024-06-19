using LabFusion.Data;
using LabFusion.Entities;

namespace LabFusion.Network;

public class PlayerSettingsData : IFusionSerializable
{
    public const int Size = sizeof(byte) + SerializedPlayerSettings.Size;

    public byte smallId;
    public SerializedPlayerSettings settings;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(settings);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        settings = reader.ReadFusionSerializable<SerializedPlayerSettings>();
    }

    public static PlayerSettingsData Create(byte smallId, SerializedPlayerSettings settings)
    {
        return new PlayerSettingsData()
        {
            smallId = smallId,
            settings = settings,
        };
    }
}

public class PlayerSettingsMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.PlayerSettings;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<PlayerSettingsData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
        {
            player.SetSettings(data.settings);
        }
    }
}