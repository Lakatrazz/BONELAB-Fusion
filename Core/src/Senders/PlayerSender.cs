using LabFusion.Exceptions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public enum PlayerActionType {
        UNKNOWN = 1 << 0,
        JUMP = 1 << 1,
        DEATH = 1 << 2,
        DYING = 1 << 3,
        RECOVERY = 1 << 4,
    }

    public enum NicknameVisibility {
        SHOW = 1 << 0,
        SHOW_WITH_PREFIX = 1 << 1,
        HIDE = 1 << 2,
    }

    public static class PlayerSender {
        public static void SendPlayerMetadataRequest(byte smallId, string key, string value) {
            using (var writer = FusionWriter.Create())
            {
                using (var data = PlayerMetadataRequestData.Create(smallId, key, value))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PlayerMetadataRequest, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendPlayerMetadataResponse(byte smallId, string key, string value) {
            // Make sure this is the server
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create()) {
                    using (var data = PlayerMetadataResponseData.Create(smallId, key, value)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerMetadataResponse, writer)) {
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
            else
                throw new ExpectedClientException();
        }

        public static void SendPlayerAction(PlayerActionType type) {
            using (var writer = FusionWriter.Create()) {
                using (var data = PlayerRepActionData.Create(PlayerIdManager.LocalSmallId, type)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepAction, writer)) {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }

            // Inform the hooks locally
            MultiplayerHooking.Internal_OnPlayerAction(PlayerIdManager.LocalId, type);
        }
    }
}
