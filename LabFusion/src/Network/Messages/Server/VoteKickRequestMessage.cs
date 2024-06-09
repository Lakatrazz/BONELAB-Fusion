using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Exceptions;

namespace LabFusion.Network
{
    public class VoteKickRequestData : IFusionSerializable
    {
        public const int Size = sizeof(byte) * 2;

        public byte smallId;
        public byte target;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(target);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            target = reader.ReadByte();
        }

        public static VoteKickRequestData Create(byte smallId, byte target)
        {
            return new VoteKickRequestData()
            {
                smallId = smallId,
                target = target,
            };
        }
    }

    public class VoteKickRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.VoteKickRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            // This should only ever be handled by the server
            if (isServerHandled)
            {
                using FusionReader reader = FusionReader.Create(bytes);
                var data = reader.ReadFusionSerializable<VoteKickRequestData>();

                // Try applying the vote
                if (VoteKickHelper.Vote(data.target, data.smallId))
                {
                    int count = VoteKickHelper.GetVoteCount(data.target);
                    int required = VoteKickHelper.GetRequiredVotes();

                    bool kick = count >= required;

                    var player = PlayerIdManager.GetPlayerId(data.target);
                    player.TryGetDisplayName(out var username);

                    if (kick)
                        NetworkHelper.KickUser(player);

                    // Send response to all players
                    using var writer = FusionWriter.Create(VoteKickResponseData.Size);
                    var responseData = VoteKickResponseData.Create(data.target, username, count, required, kick);
                    writer.Write(responseData);

                    using var message = FusionMessage.Create(NativeMessageTag.VoteKickResponse, writer);
                    MessageSender.BroadcastMessageExcept(data.target, NetworkChannel.Reliable, message, false);
                }
            }
            else
                throw new ExpectedServerException();
        }
    }
}
