using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;

using System;
using LabFusion.Senders;
using LabFusion.Exceptions;
using LabFusion.Extensions;

namespace LabFusion.Network
{
    public class VoteKickResponseData : IFusionSerializable, IDisposable {
        public const int Size = sizeof(byte) * 2 + sizeof(int) * 2;

        public byte smallId;
        public string username;

        public int votes;
        public int requiredVotes;

        public bool wasKicked;

        public void Serialize(FusionWriter writer) {
            writer.Write(smallId);
            writer.Write(username);

            writer.Write(votes);
            writer.Write(requiredVotes);

            writer.Write(wasKicked);
        }

        public void Deserialize(FusionReader reader) {
            smallId = reader.ReadByte();
            username = reader.ReadString();

            votes = reader.ReadInt32();
            requiredVotes = reader.ReadInt32();

            wasKicked = reader.ReadBoolean();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static VoteKickResponseData Create(byte smallId, string username, int votes, int requiredVotes, bool wasKicked) {
            return new VoteKickResponseData() {
                smallId = smallId,
                username = username,
                votes = votes,
                requiredVotes = requiredVotes,
                wasKicked = wasKicked
            };
        }
    }

    public class VoteKickResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.VoteKickResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            // This should only ever be handled by clients
            if (!isServerHandled) {
                using FusionReader reader = FusionReader.Create(bytes);
                using var data = reader.ReadFusionSerializable<VoteKickResponseData>();

                // Don't show if we are the one being vote kicked
                // This message should not be sent to us but just incase
                if (data.smallId == PlayerIdManager.LocalSmallId)
                    return;

                // Send notifications
                // Was kicked
                if (data.wasKicked) {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Vote Kick",
                        message = $"{data.username} was kicked. ({data.votes}/{data.requiredVotes} Votes)",
                        type = NotificationType.SUCCESS,
                        isPopup = true,
                        isMenuItem = false,
                        showTitleOnPopup = true,
                    });
                }
                // Kick in progress
                else {
                    FusionNotifier.Send(new FusionNotification() {
                        title = "Vote Kick",
                        message = $"A player has voted to kick {data.username}. ({data.votes}/{data.requiredVotes} Votes)",
                        type = NotificationType.INFORMATION,
                        isPopup = true,
                        isMenuItem = false,
                        showTitleOnPopup = true,
                    });
                }
            }
            else
                throw new ExpectedClientException();
        }
    }
}
