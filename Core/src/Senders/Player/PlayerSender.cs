using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Senders {
    public enum PlayerActionType {
        UNKNOWN = 1 << 0,
        JUMP = 1 << 1,
        DEATH = 1 << 2,
        DYING = 1 << 3,
        RECOVERY = 1 << 4,
        DEATH_BY_OTHER_PLAYER = 1 << 5,
    }

    public enum NicknameVisibility {
        SHOW = 1 << 0,
        SHOW_WITH_PREFIX = 1 << 1,
        HIDE = 1 << 2,
    }

    public static class PlayerSender {
        public static void SendPlayerAvatar(SerializedAvatarStats stats, string barcode) {
            if (!NetworkInfo.HasServer)
                return;

            using (FusionWriter writer = FusionWriter.Create(PlayerRepAvatarData.DefaultSize)) {
                using (PlayerRepAvatarData data = PlayerRepAvatarData.Create(PlayerIdManager.LocalSmallId, stats, barcode)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepAvatar, writer)) {
                        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendPlayerVoiceChat(byte[] voiceData) {
            if (!NetworkInfo.HasServer)
                return;

            using (var writer = FusionWriter.Create(PlayerVoiceChatData.Size + voiceData.Length))
            {
                using (var data = PlayerVoiceChatData.Create(PlayerIdManager.LocalSmallId, voiceData))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PlayerVoiceChat, writer))
                    {
                        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.VoiceChat, message);
                    }
                }
            }
        }

        public static void SendPlayerTeleport(byte target, Vector3 position)
        {
            if (!NetworkInfo.IsServer)
                return;

            using (var writer = FusionWriter.Create(PlayerRepTeleportData.Size))
            {
                using (var data = PlayerRepTeleportData.Create(target, position))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepTeleport, writer))
                    {
                        MessageSender.SendFromServer(target, NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendPlayerDamage(byte target, float damage) {
            using (var writer = FusionWriter.Create(PlayerRepDamageData.Size))
            {
                using (var data = PlayerRepDamageData.Create(PlayerIdManager.LocalSmallId, target, damage))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepDamage, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

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

        public static void SendPlayerAction(PlayerActionType type, byte? otherPlayer = null) {
            using (var writer = FusionWriter.Create()) {
                using (var data = PlayerRepActionData.Create(PlayerIdManager.LocalSmallId, type, otherPlayer)) {
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
