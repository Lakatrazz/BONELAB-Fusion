using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class BodyLogToggleData : INetSerializable
{
    public const int Size = sizeof(byte) * 2;

    public byte smallId;
    public bool isEnabled;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref isEnabled);
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