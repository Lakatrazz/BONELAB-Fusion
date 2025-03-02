using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class BodyLogEffectData : INetSerializable
{
    public const int Size = sizeof(byte);

    public byte smallId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
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