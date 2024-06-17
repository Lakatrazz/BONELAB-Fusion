using LabFusion.Data;
using LabFusion.Representation;

using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Network
{
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

        public PlayerRep GetRep()
        {
            if (PlayerRepManager.TryGetPlayerRep(smallId, out var rep))
                return rep;
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
    public class PlayerRepReleaseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepRelease;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<PlayerRepReleaseData>();
            var rep = data.GetRep();

            if (rep != null)
            {
                rep.DetachObject(data.handedness);
            }

            // Send message to other clients if server
            if (isServerHandled)
            {
                if (data.smallId != 0)
                {
                    using var message = FusionMessage.Create(Tag.Value, bytes);
                    MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
                }
            }
        }
    }
}
