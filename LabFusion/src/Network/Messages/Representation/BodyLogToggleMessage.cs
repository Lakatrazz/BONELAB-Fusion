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
public class BodyLogToggleMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.BodyLogToggle;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<BodyLogToggleData>();

        // Set the enabled state of the body log
        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player) && !player.NetworkEntity.IsOwner)
        {
            player.SetBallEnabled(data.isEnabled);
        }
    }
}