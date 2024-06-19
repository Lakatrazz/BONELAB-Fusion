using LabFusion.Data;
using LabFusion.Entities;

namespace LabFusion.Network
{
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
    public class BodyLogEffectMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.BodyLogEffect;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<BodyLogEffectData>();

            // Play the effect
            if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player) && !player.NetworkEntity.IsOwner)
            {
                player.PlayPullCordEffects();
            }

            // Bounce the message back
            if (NetworkInfo.IsServer)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
            }
        }
    }
}
