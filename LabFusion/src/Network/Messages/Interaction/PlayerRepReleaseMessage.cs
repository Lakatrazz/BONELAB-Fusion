using LabFusion.Data;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Network;

public class PlayerRepReleaseData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2;

    public byte smallId;
    public Handedness handedness;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write((byte)handedness);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        handedness = (Handedness)reader.ReadByte();
    }

    public NetworkPlayer GetPlayer()
    {
        if (NetworkPlayerManager.TryGetPlayer(smallId, out var player))
        {
            return player;
        }

        return null;
    }

    public static PlayerRepReleaseData Create(byte smallId, Handedness handedness)
    {
        return new PlayerRepReleaseData()
        {
            smallId = smallId,
            handedness = handedness
        };
    }
}

[Net.DelayWhileTargetLoading]
public class PlayerRepReleaseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepRelease;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepReleaseData>();

        var player = data.GetPlayer();

        if (player == null)
        {
            return;
        }

        if (!player.HasRig)
        {
            return;
        }

        player.Grabber.Detach(data.handedness);
    }
}