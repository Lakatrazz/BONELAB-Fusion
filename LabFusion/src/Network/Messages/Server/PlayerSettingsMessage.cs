using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PlayerSettingsData : INetSerializable
{
    public const int Size = sizeof(byte) + SerializedPlayerSettings.Size;

    public byte smallId;
    public SerializedPlayerSettings settings;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref settings);
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