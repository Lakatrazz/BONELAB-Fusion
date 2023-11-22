using System;
using LabFusion.Data;
using LabFusion.Representation;

namespace LabFusion.Network
{
    public class PlayerRepRagdollData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 2;

        public byte smallId;
        public bool isRagdoll;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(isRagdoll);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            isRagdoll = reader.ReadBoolean();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepRagdollData Create(byte smallId, bool isRagdoll)
        {
            return new PlayerRepRagdollData
            {
                smallId = smallId,
                isRagdoll = isRagdoll,
            };
        }
    }

    [Net.SkipHandleWhileLoading]
    public class PlayerRepRagdollMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepRagdoll;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<PlayerRepRagdollData>();

            // Send message to other clients if server
            if (NetworkInfo.IsServer && isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            }
            else if (PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep))
            {
                rep.SetRagdoll(data.isRagdoll);
            }
        }
    }
}
