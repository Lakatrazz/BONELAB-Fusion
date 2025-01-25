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

public class PlayerSettingsMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerSettings;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerSettingsData>();

        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
        {
            player.SetSettings(data.settings);
        }
    }
}