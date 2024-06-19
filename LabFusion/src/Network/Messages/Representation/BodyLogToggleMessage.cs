using LabFusion.Data;
using LabFusion.Entities;

namespace LabFusion.Network;

public class BodyLogToggleData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2;

    public byte smallId;
    public bool isEnabled;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(isEnabled);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        isEnabled = reader.ReadBoolean();
    }

    public static BodyLogToggleData Create(byte smallId, bool isEnabled)
    {
        return new BodyLogToggleData()
        {
            smallId = smallId,
            isEnabled = isEnabled,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class BodyLogToggleMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.BodyLogToggle;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<BodyLogToggleData>();

        // Set the enabled state of the body log
        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player) && !player.NetworkEntity.IsOwner)
        {
            player.SetBallEnabled(data.isEnabled);
        }

        // Bounce the message back
        if (NetworkInfo.IsServer)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
        }
    }
}