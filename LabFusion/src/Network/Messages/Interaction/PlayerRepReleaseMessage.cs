using LabFusion.Network.Serialization;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Network;

public class PlayerRepReleaseData : INetSerializable
{
    public const int Size = sizeof(byte);

    public Handedness Handedness;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Handedness, Precision.OneByte);
    }
}

[Net.SkipHandleWhileLoading]
public class PlayerRepReleaseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepRelease;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepReleaseData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        if (!NetworkPlayerManager.TryGetPlayer(sender.Value, out var player))
        {
            return;
        }

        if (!player.HasRig)
        {
            return;
        }

        player.Grabber.Detach(data.Handedness);
    }
}