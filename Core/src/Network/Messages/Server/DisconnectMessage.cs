using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Representation;

namespace LabFusion.Network
{
    public class DisconnectMessageData : IFusionSerializable
    {
        public ulong longId;
        public string reason;

        public static DisconnectMessageData Create(ulong longId, string reason = "")
        {
            return new DisconnectMessageData()
            {
                longId = longId,
                reason = reason,
            };
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(longId);
            writer.Write(reason);
        }

        public void Deserialize(FusionReader reader)
        {
            longId = reader.ReadUInt64();
            reason = reader.ReadString();
        }
    }

    public class DisconnectMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.Disconnect;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            // Make sure this message isn't handled by the server
            if (!isServerHandled)
            {
                using var reader = FusionReader.Create(bytes);
                var data = reader.ReadFusionSerializable<DisconnectMessageData>();

                // If this is our id, disconnect ourselves
                if (data.longId == PlayerIdManager.LocalLongId)
                {
                    NetworkHelper.Disconnect(data.reason);
                }
                // Otherwise, disconnect the other person in the lobby
                else
                {
                    InternalServerHelpers.OnUserLeave(data.longId);
                }
            }
            else
                throw new ExpectedClientException();
        }
    }
}
