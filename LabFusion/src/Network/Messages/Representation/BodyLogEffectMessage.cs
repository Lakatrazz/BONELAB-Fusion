using LabFusion.Data;
using LabFusion.Entities;

namespace LabFusion.Network;

public class BodyLogEffectData : IFusionSerializable
{
    public const int Size = sizeof(byte);

    public byte smallId;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
    }

    public static BodyLogEffectData Create(byte smallId)
    {
        return new BodyLogEffectData()
        {
            smallId = smallId,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class BodyLogEffectMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.BodyLogEffect;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<BodyLogEffectData>();

        // Play the effect
        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player) && !player.NetworkEntity.IsOwner)
        {
            player.PlayPullCordEffects();
        }
    }
}